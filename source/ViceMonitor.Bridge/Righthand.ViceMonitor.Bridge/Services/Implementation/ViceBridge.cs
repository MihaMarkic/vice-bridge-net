﻿using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Services.Abstract;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation
{
    public sealed class ViceBridge: IViceBridge
    {
        readonly ILogger<ViceBridge> logger;
        readonly ResponseBuilder responseBuilder;
        CancellationTokenSource? cts;
        Task? loop;
        TaskCompletionSource? tcs;
        uint currentRequestId = 0;
        readonly ArrayPool<byte> byteArrayPool = ArrayPool<byte>.Shared;
        readonly ConcurrentQueue<IViceCommand> commands = new ConcurrentQueue<IViceCommand>();
        public Task? RunnerTask => tcs?.Task;
        /// <summary>
        /// Triggers on all unbound responses.
        /// </summary>
        /// <threadsafety>Runs on any thread</threadsafety>
        public event EventHandler<ViceResponseEventArgs>? ViceResponse;
        public event EventHandler<ConnectedChangedEventArgs>? ConnectedChanged;
        public bool IsConnected { get; private set; }
        public ViceBridge(ILogger<ViceBridge> logger, ResponseBuilder responseBuilder)
        {
            this.logger = logger;
            this.responseBuilder = responseBuilder;
        }
        void OnViceResponse(ViceResponseEventArgs e) => ViceResponse?.Invoke(this, e);
        void OnConnectedChanged(ConnectedChangedEventArgs e) => ConnectedChanged?.Invoke(this, e);
        async Task WaitForPort(int port, CancellationToken ct = default)
        {
            bool isAvailable;
            do
            {
                ct.ThrowIfCancellationRequested();
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var info = properties.GetActiveTcpListeners();

                isAvailable = info.Any(tl => tl.Port == port);
                if (!isAvailable)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), ct).ConfigureAwait(false);
                }
            } while (!isAvailable);
        }
        public bool IsRunning => cts is not null && loop is not null;
        public void Start(IPAddress address, int port = 6502)
        {
            if (!IsRunning)
            {
                logger.LogDebug("Start called");
                tcs = new TaskCompletionSource();
                cts = new CancellationTokenSource();
                var task = Task.Factory.StartNew(
                    () => StartAsync(address, port, cts.Token), 
                    cancellationToken: cts.Token, 
                    creationOptions: TaskCreationOptions.LongRunning,
                    scheduler: TaskScheduler.Default);
                loop = task.Unwrap();
            }
            else
            {
                logger.LogInformation("Already running");
            }

        }
        async Task StartAsync(IPAddress address, int port, CancellationToken ct)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                while (true)
                {
                    logger.LogInformation("Starting");
                    try
                    {
                        logger.LogDebug("Waiting for available port");
                        await WaitForPort(port, ct).ConfigureAwait(false);
                        logger.LogDebug("Port acquired");
                        socket.Connect("localhost", port);
                        logger.LogDebug("Port connected");
                        while (socket.Connected)
                        {
                            IsConnected = true;
                            OnConnectedChanged(new ConnectedChangedEventArgs(true));
                            await LoopAsync(socket, ct);
                            await Task.Delay(500, ct).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("Finishing loop");
                        tcs!.SetResult();
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unknown exception occured");
                    }
                    finally
                    {
                        IsConnected = false;
                        OnConnectedChanged(new ConnectedChangedEventArgs(false));
                        logger.LogInformation("Ending");
                    }
                }
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
        }
        async Task<ViceResponse> WaitUntilMatchesResponseAsync(Socket socket, uint targetRequestId, CancellationToken ct)
        {
            logger.LogDebug($"Waiting for request id {targetRequestId}");
            while(true)
            {
                ct.ThrowIfCancellationRequested();
                (var response, var requestId) = await GetResponseAsync(socket, ct).ConfigureAwait(false);
                if (requestId == targetRequestId)
                {
                    logger.LogDebug($"Found matching request id {targetRequestId}");
                    return response;
                }
                else
                {
                    if (requestId != Constants.BroadcastRequestId)
                    {
                        logger.LogWarning($"Got unmatched response with non broadcast request id {requestId:x8}");
                    }
                    OnViceResponse(new ViceResponseEventArgs(response));
                }
            }
        }
        public void EnqueCommand(IViceCommand command)
        {
            commands.Enqueue(command);
        }
        async Task LoopAsync(Socket socket, CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                while (commands.TryDequeue(out var command))
                {
                    logger.LogDebug($"Will process command {currentRequestId} of {command.GetType().Name}");
                    await SendCommandAsync(socket, currentRequestId, command, ct).ConfigureAwait(false);
                    var response = await WaitUntilMatchesResponseAsync(socket, currentRequestId, ct).ConfigureAwait(false);
                    logger.LogDebug($"Command {currentRequestId} of {command.GetType().Name} got response with result {response.ErrorCode}");
                    command.SetResult(response);
                    currentRequestId++;
                }
                if (socket.Available > 0)
                {
                    logger.LogDebug("Will process unbound response");
                    (var response, var requestId) = await GetResponseAsync(socket, ct).ConfigureAwait(false);
                    OnViceResponse(new ViceResponseEventArgs(response));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ct"></param>
        /// <returns>An instance of <see cref="ManagedBuffer"/> that has to be disposed after use.</returns>
        async Task<(ViceResponse Response, uint RequestId)> GetResponseAsync(Socket socket, CancellationToken ct)
        {
            using (var headerBuffer = byteArrayPool.GetBuffer(12))
            {
                await ReadByteArrayAsync(socket, headerBuffer, ct).ConfigureAwait(false);
                uint responseBodyLength = responseBuilder.GetReponseBodyLength(headerBuffer.Data.AsSpan());
                logger.LogDebug($"Response body length is {responseBodyLength}");
                using (var bodyBuffer = byteArrayPool.GetBuffer(responseBodyLength))
                {
                    await ReadByteArrayAsync(socket, bodyBuffer, ct);
                    var result = responseBuilder.Build(headerBuffer.Data.AsSpan(), bodyBuffer.Data.AsSpan());
                    logger.LogDebug($"Response is {result.Response?.GetType().Name} with RequestId {result.RequestId}");
                    return result;
                }
            }
        }
        async Task SendCommandAsync(Socket socket, uint requestId, IViceCommand command, CancellationToken ct)
        {
            logger.LogDebug($"Sending command {command.GetType().Name} with RequestId {requestId}");

            (var buffer, var length) = command.GetBinaryData(requestId);
            try
            {
                logger.LogDebug($"Sending command length is {length}");
                await SendByteArrayAsync(socket, buffer.Data, (int)length, ct).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
        }
        async Task ReadByteArrayAsync(Socket socket, ManagedBuffer buffer, CancellationToken ct = default)
        {
            int i = 0;
            var dataSpan = buffer.Data.AsMemory();
            do
            {
                i += await socket.ReceiveAsync(dataSpan[i..(int)buffer.Size], SocketFlags.None).ConfigureAwait(false);
            }
            while (i < buffer.Size);
        }
        async Task SendByteArrayAsync(Socket socket, byte[] data, int length, CancellationToken ct)
        {
            int i = 0;
            var dataSpan = data.AsMemory();
            do
            {
                int sent = await socket.SendAsync(dataSpan[i..length], SocketFlags.None, ct);
                if (sent > 0)
                {
                    i += sent;
                }
                else
                {
                    await Task.Delay(10, ct);
                }
                ct.ThrowIfCancellationRequested();
            }
            while (i < length);
        }

        public async ValueTask DisposeAsync()
        {
            if (cts is not null && loop is not null)
            {
                logger.LogDebug("Dispose async");
                cts.Cancel();
                try
                {
                    await loop;
                }
                catch (OperationCanceledException)
                { }
                logger.LogDebug("Disposed async");
                cts = null;
                loop = null;
            }
            else
            {
                logger.LogDebug("Nothing to dispose async");
            }
        }

        public void Dispose()
        {
            logger.LogDebug("Dispose");
            cts?.Cancel();
        }
    }
}
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Exceptions;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation
{
    /// <summary>
    /// Bridge that allows communication with VICE running on the same machine.
    /// </summary>
    /// <remarks>In future connection to remote machines will be enabled.</remarks>
    /// <threadsafety>Class members are not thread safe unless explicitly stated.</threadsafety>
    public sealed class ViceBridge: IViceBridge
    {
        readonly object sync = new object();
        readonly ILogger<ViceBridge> logger;
        readonly ResponseBuilder responseBuilder;
        CancellationTokenSource? cts;
        Task? loop;
        TaskCompletionSource? tcs;
        uint currentRequestId = 0;
        readonly ArrayPool<byte> byteArrayPool = ArrayPool<byte>.Shared;
        readonly ConcurrentQueue<IViceCommand> commands = new ();
        bool isConnected;
        bool isRunning;
        public bool IsStarted => tcs is not null && cts is not null;
        /// <inheritdoc />
        public Task? RunnerTask => tcs?.Task;
        /// <inheritdoc />
        public event EventHandler<ViceResponseEventArgs>? ViceResponse;
        /// <inheritdoc />
        public event EventHandler<ConnectedChangedEventArgs>? ConnectedChanged;
        /// <summary>
        /// Creates an instance of <see cref="ViceBridge"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="responseBuilder">Type responsible for building response types.</param>
        public ViceBridge(ILogger<ViceBridge> logger, ResponseBuilder responseBuilder)
        {
            this.logger = logger;
            this.responseBuilder = responseBuilder;
        }
        /// <inheritdoc />
        /// <threadsafety>Property is thread safe.</threadsafety>
        public bool IsConnected 
        { 
            get { lock (sync) { return isConnected; } }
            private set { lock (sync) { isConnected = value; } }
        }
        void OnViceResponse(ViceResponseEventArgs e) => ViceResponse?.Invoke(this, e);
        void OnConnectedChanged(ConnectedChangedEventArgs e) => ConnectedChanged?.Invoke(this, e);
        static async Task WaitForPort(int port, CancellationToken ct = default)
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
                    await Task.Delay(TimeSpan.FromSeconds(.5), ct).ConfigureAwait(false);
                }
            } while (!isAvailable);
        }
        /// <inheritdoc />
        /// <threadsafety>Property is thread safe.</threadsafety>
        public bool IsRunning
        {
            get { lock (sync) { return isRunning; } }
            private set { lock (sync) { isRunning = value; } }
        }
        /// <inheritdoc />
        public void Start(int port = 6502)
        {
            if (!IsStarted)
            {
                logger.LogDebug("Start called");
                tcs = new TaskCompletionSource();
                lock (sync)
                {
                    cts = new CancellationTokenSource();
                }
                var task = Task.Factory.StartNew(
                    () => StartAsync(port, cts.Token), 
                    cancellationToken: cts.Token, 
                    creationOptions: TaskCreationOptions.LongRunning,
                    scheduler: TaskScheduler.Default);
                loop = task.Unwrap();
            }
            else
            {
                logger.LogWarning("Already running");
            }
        }
        public async Task StopAsync()
        {
            if (IsStarted)
            {
                lock (sync)
                {
                    cts?.Cancel();
                }
                var runner = RunnerTask;
                if (runner is not null)
                {
                    await runner;
                }
            }
        }
        async Task StartAsync(int port, CancellationToken ct)
        {
            try
            {
                Socket? socket;
                while (true)
                {
                    logger.LogInformation("Starting");
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        logger.LogDebug("Waiting for available port");
                        await WaitForPort(port, ct).ConfigureAwait(false);
                        logger.LogDebug("Port acquired");
                        socket.Connect("localhost", port);
                        logger.LogDebug("Port connected");
                        while (socket.Connected)
                        {
                            lock (sync)
                            {
                                if (!isConnected)
                                {
                                    isConnected = true;
                                    OnConnectedChanged(new ConnectedChangedEventArgs(true));
                                }
                            }
                            await LoopAsync(socket, ct);
                            await Task.Delay(500, ct).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("Finishing loop");
                    }
                    catch (SocketDisconnectedException)
                    {
                        logger.LogWarning("Socket disconnected");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unknown exception occurred");
                    }
                    finally
                    {
                        IsConnected = false;
                        OnConnectedChanged(new ConnectedChangedEventArgs(false));
                        logger.LogInformation("Ending");
                        if (socket.Connected)
                        {
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            socket.Dispose();
                        }
                        tcs!.SetResult();
                    }
                }
            }
            finally
            {
                lock (sync)
                {
                    cts = null;
                }
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
        /// <inheritdoc />
        /// <threadsafety>Method is thread safe.</threadsafety>
        public T EnqueueCommand<T>(T command)
            where T: IViceCommand
        {
            commands.Enqueue(command);
            return command;
        }
        /// <summary>
        /// Checks if socket is still connected
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        /// <remarks>https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c</remarks>
        bool CheckIfSocketConnected(Socket socket)
        {
            bool part1 = socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        async Task LoopAsync(Socket socket, CancellationToken ct)
        {
            isRunning = true;
            try
            {
                while (true)
                {
                    if (!CheckIfSocketConnected(socket))
                    {
                        throw new SocketDisconnectedException("Socket is disconnected");
                    }
                    ct.ThrowIfCancellationRequested();
                    while (commands.TryDequeue(out var command))
                    {
                        logger.LogDebug($"Will process command {currentRequestId} of {command.GetType().Name} with request id {currentRequestId}");
                        await SendCommandAsync(socket, currentRequestId, command, ct).ConfigureAwait(false);
                        (command as IDisposable)?.Dispose();
                        ViceResponse response;
                        switch (command)
                        {
                            // list is a special case that collects matching CheckpointInfoResponses
                            case CheckpointListCommand listCommand:
                                var info = ImmutableArray<CheckpointInfoResponse>.Empty;
                                while ((response = await WaitUntilMatchesResponseAsync(socket, currentRequestId, ct).ConfigureAwait(false)) is CheckpointInfoResponse infoResponse)
                                {
                                    info = info.Add(infoResponse);
                                }
                                var listResponse = (CheckpointListResponse)response;
                                response = listResponse with { Info = info };
                                break;
                            default:
                                response = await WaitUntilMatchesResponseAsync(socket, currentRequestId, ct).ConfigureAwait(false);
                                logger.LogDebug($"Command {currentRequestId} of {command.GetType().Name} got response with result {response.ErrorCode}");
                                break;
                        }
                        command.SetResult(response);
                        currentRequestId++;
                    }
                    if (socket.Available > 0)
                    {
                        logger.LogDebug("Will process unbound response");
                        (var response, _) = await GetResponseAsync(socket, ct).ConfigureAwait(false);
                        OnViceResponse(new ViceResponseEventArgs(response));
                    }
                    else
                    {
                        // prevents too intensive pooling
                        await Task.Delay(500, ct);
                    }
                }
            }
            finally
            {
                IsRunning = false;
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
                uint responseBodyLength = responseBuilder.GetResponseBodyLength(headerBuffer.Data.AsSpan());
                logger.LogDebug($"Response body length is {responseBodyLength:#,##0}B");
                (ViceResponse Response, uint RequestId) result;
                if (responseBodyLength > 0)
                {
                    using (var bodyBuffer = byteArrayPool.GetBuffer(responseBodyLength))
                    {
                        await ReadByteArrayAsync(socket, bodyBuffer, ct);
                        result = responseBuilder.Build(headerBuffer.Data.AsSpan(), bodyBuffer.Data.AsSpan());
                    }
                }
                else
                {
                    result = responseBuilder.Build(headerBuffer.Data.AsSpan(), Array.Empty<byte>());
                }
                logger.LogDebug($"Response is {result.Response?.GetType().Name} with RequestId {result.RequestId}");
                return result;
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
        static async Task ReadByteArrayAsync(Socket socket, ManagedBuffer buffer, CancellationToken ct = default)
        {
            int i = 0;
            var dataSpan = buffer.Data.AsMemory();
            do
            {
                i += await socket.ReceiveAsync(dataSpan[i..(int)buffer.Size], SocketFlags.None, ct).ConfigureAwait(false);
            }
            while (i < buffer.Size);
        }
        static async Task SendByteArrayAsync(Socket socket, byte[] data, int length, CancellationToken ct)
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
        /// <summary>
        /// Asynchronously releases all resources used by the <see cref="ViceBridge"/>.
        /// </summary>
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
                lock (sync)
                {
                    cts = null;
                    loop = null;
                }
            }
            else
            {
                logger.LogDebug("Nothing to dispose async");
            }
        }
        /// <summary>
        /// Releases all resources used by the <see cref="ViceBridge"/>.
        /// </summary>
        public void Dispose()
        {
            logger.LogDebug("Dispose");
            cts?.Cancel();
        }
    }
}

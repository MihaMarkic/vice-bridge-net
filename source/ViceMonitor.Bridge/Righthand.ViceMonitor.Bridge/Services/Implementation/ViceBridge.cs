using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Exceptions;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using System.Buffers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation
{
    /// <summary>
    /// Bridge that allows communication with VICE running on the same machine.
    /// </summary>
    /// <remarks>In future connection to remote machines will be enabled.</remarks>
    /// <threadsafety>Class members are not thread safe unless explicitly stated.</threadsafety>
    public sealed class ViceBridge: IViceBridge
    {
        private record EnqueuedCommand(IViceCommand Command, bool ResumeOnStopped);
        private readonly object _sync = new ();
        private readonly ILogger<ViceBridge> _logger;
        private readonly ResponseBuilder _responseBuilder;
        private CancellationTokenSource? _cts;
        private Task? _loop;
        private TaskCompletionSource? _tcs;
        private uint _currentRequestId;
        private readonly ArrayPool<byte> _byteArrayPool = ArrayPool<byte>.Shared;
        private BufferBlock<EnqueuedCommand>? _commands;
        private bool _isConnected;
        private bool _isRunning;
        /// <inheritdoc/>
        public bool IsStarted => _tcs is not null && _cts is not null;
        /// <inheritdoc />
        public Task? RunnerTask => _tcs?.Task;
        /// <inheritdoc />
        public event EventHandler<ViceResponseEventArgs>? ViceResponse;
        /// <inheritdoc />
        public event EventHandler<ConnectedChangedEventArgs>? ConnectedChanged;
        /// <summary>
        /// Log of performance data.
        /// </summary>
        public IPerformanceProfiler PerformanceProfiler { get; }
        /// <summary>
        /// Service that keeps history of messages.
        /// </summary>
        public IMessagesHistory MessagesHistory { get; }
        /// <summary>
        /// Creates an instance of <see cref="ViceBridge"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="responseBuilder">Type responsible for building response types.</param>
        /// <param name="performanceProfiler">Performance profiler service.</param>
        /// <param name="messagesHistory">Messages history service.</param>
        public ViceBridge(ILogger<ViceBridge> logger, ResponseBuilder responseBuilder, 
            IPerformanceProfiler performanceProfiler, IMessagesHistory messagesHistory)
        {
            _logger = logger;
            _responseBuilder = responseBuilder;
            PerformanceProfiler = performanceProfiler;
            MessagesHistory = messagesHistory;
        }
        /// <inheritdoc />
        /// <threadsafety>Property is thread safe.</threadsafety>
        public bool IsConnected 
        { 
            get { lock (_sync) { return _isConnected; } }
            private set { lock (_sync) { _isConnected = value; } }
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
            get { lock (_sync) { return _isRunning; } }
            private set { lock (_sync) { _isRunning = value; } }
        }
        /// <inheritdoc />
        public void Start(int port = 6502)
        {
            if (!IsStarted)
            {
                _logger.LogDebug("Start called");
                _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                CancellationToken token;
                lock (_sync)
                {
                    _cts = new CancellationTokenSource();
                    token = _cts.Token;
                }
                _commands = new BufferBlock<EnqueuedCommand>();
                var task = Task.Factory.StartNew(
                    () => StartAsync(port, _commands, token), 
                    cancellationToken: token, 
                    creationOptions: TaskCreationOptions.LongRunning,
                    scheduler: TaskScheduler.Default);
                _loop = task.Unwrap();
            }
            else
            {
                _logger.LogWarning("Already running");
            }
        }
        /// <inheritdoc/>
        public async Task StopAsync(bool waitForQueueToProcess)
        {
            if (IsStarted)
            {
                if (waitForQueueToProcess)
                {
                    if (_commands is null)
                    {
                        throw new Exception("Commands queue is not initialized");
                    }
                    _commands?.Complete();
                }
                else
                {
                    lock (_sync)
                    {
                        _cts?.Cancel();
                    }
                }
                var runner = RunnerTask;
                if (runner is not null)
                {
                    try
                    {
                        await runner;
                    }
                    catch (OperationCanceledException e)
                    {
                        // it should throw an OperationCanceledException when cancelled
                        // don't propagate, just exit
                    }
                }
            }
        }

        private async Task StartAsync(int port, ISourceBlock<EnqueuedCommand> source, CancellationToken ct)
        {
            Thread.CurrentThread.Name = "Bridge loop";
            try
            {
                bool run = true;
                while (run)
                {
                    _logger.LogInformation("Starting");
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        _logger.LogDebug("Waiting for available port");
                        await WaitForPort(port, ct).ConfigureAwait(false);
                        _logger.LogDebug("Port acquired");
                        await socket.ConnectAsync("localhost", port, ct);
                        _logger.LogDebug("Port connected");
                        if (socket.Connected)
                        {
                            lock (_sync)
                            {
                                if (!_isConnected)
                                {
                                    _isConnected = true;
                                    OnConnectedChanged(new ConnectedChangedEventArgs(true));
                                }
                            }
                            await LoopAsync(socket, source, ct);
                            run = false;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Finishing loop");
                        throw;
                    }
                    catch (SocketDisconnectedException)
                    {
                        _logger.LogWarning("Socket disconnected");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unknown exception occurred");
                    }
                    finally
                    {
                        IsConnected = false;
                        OnConnectedChanged(new ConnectedChangedEventArgs(false));
                        _logger.LogInformation("Ending");
                        if (socket.Connected)
                        {
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            socket.Dispose();
                        }
                        _tcs!.SetResult();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Exiting loop due to cancellation");
            }
            finally
            {
                _commands = null;
                lock (_sync)
                {
                    _cts = null;
                }
            }
        }
        /// <summary>
        /// Defines last response statuses of interest.
        /// </summary>
        public enum LastStatusResponse
        {
            /// <summary>
            /// Stopped execution.
            /// </summary>
            Stopped,
            /// <summary>
            /// Resumed execution.
            /// </summary>
            Resumed,
            /// <summary>
            /// No other status from the list was received.
            /// </summary>
            None
        }

        private async Task<(ViceResponse Response, LastStatusResponse LastStatusResponse)> WaitUntilMatchesResponseAsync(Socket socket, uint targetRequestId, CancellationToken ct)
        {
            _logger.LogDebug($"Waiting for request id {targetRequestId}");
            LastStatusResponse lastStatusResponse = LastStatusResponse.None;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var (response, requestId) = await GetResponseAsync(socket, ct).ConfigureAwait(false);
                if (requestId == targetRequestId)
                {
                    if (PerformanceProfiler.IsEnabled)
                    {
                        PerformanceProfiler.Add(new ResponseReadEvent(response.GetType(), IsNested: false, PerformanceProfiler.Ticks));
                    }
                    _logger.LogDebug($"Found matching request id {targetRequestId}");
                    return (response, lastStatusResponse);
                }
                else
                {
                    switch(response)
                    {
                        case StoppedResponse:
                            lastStatusResponse = LastStatusResponse.Stopped; 
                            break;
                        case ResumedResponse:
                            lastStatusResponse = LastStatusResponse.Resumed;
                            break;
                    }
                    if (PerformanceProfiler.IsEnabled)
                    {
                        PerformanceProfiler.Add(new ResponseReadEvent(response.GetType(), IsNested: true, PerformanceProfiler.Ticks));
                    }
                    if (requestId != Constants.BroadcastRequestId)
                    {
                        _logger.LogDebug($"Got unmatched response with non broadcast request id {requestId:x8}");
                    }
                    MessagesHistory.AddsResponseOnly(response);
                    OnViceResponse(new ViceResponseEventArgs(response));
                }
            }
        }
        /// <inheritdoc />
        /// <threadsafety>Method is thread safe.</threadsafety>
        public T EnqueueCommand<T>(T command, bool resumeOnStopped = false)
            where T: IViceCommand
        {
            if (_commands is null)
            {
                throw new Exception("Service is not running");
            }
            var errors = command.CollectErrors();
            if (errors.Length > 0)
            {
                throw new Exception(string.Join(Environment.NewLine, errors));
            }
            _commands.Post(new EnqueuedCommand(command, resumeOnStopped));
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
        async Task LoopAsync(Socket socket, ISourceBlock<EnqueuedCommand> source, CancellationToken ct)
        {
            _isRunning = true;
            try
            {
                while (true)
                {
                    var commandAvailableTask = source.OutputAvailableAsync(ct);
                    var dataAvailableTask = socket.WaitForDataAsync(ct);
                    if (PerformanceProfiler.IsEnabled)
                    {
                        PerformanceProfiler.Add(new StartListeningEvent(PerformanceProfiler.Ticks));
                    }
                    var task = await Task.WhenAny(commandAvailableTask, dataAvailableTask).ConfigureAwait(false);
                    if (PerformanceProfiler.IsEnabled)
                    {
                        PerformanceProfiler.Add(new DataAvailableEvent(
                        task == commandAvailableTask ? PerformanceDataType.Command : PerformanceDataType.Response,
                        PerformanceProfiler.Ticks));
                    }
                    ct.ThrowIfCancellationRequested();
                    if (task == commandAvailableTask)
                    {
                        bool isHandlingSuccessful = true;
                        var (command, resumeOnStopped) = await source.ReceiveAsync(ct);
                        var (response, lastStatusResponse) = await SendCommandAndWaitForResponse(socket, command, ct).ConfigureAwait(false);

                        if (resumeOnStopped && lastStatusResponse == LastStatusResponse.Stopped)
                        {
                            _logger.LogDebug("Resuming VICE");
                            var exitCommand = new ExitCommand();
                            var (exitResponse, exitLastStatusResponse) = await SendCommandAndWaitForResponse(socket, exitCommand, ct)
                                .ConfigureAwait(false);
                            _logger.LogDebug("Resumed VICE with status {status}", exitResponse.ErrorCode);
                            if (exitLastStatusResponse != LastStatusResponse.Resumed)
                            {
                                // waits two seconds before it times out
                                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                                using (CancellationTokenSource linkedCts =
                                    CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token))
                                {
                                    try
                                    {
                                        await WaitForResumedResponse(socket, linkedCts.Token);
                                    }
                                    // handle timeouts only, propagate other exceptions
                                    catch (OperationCanceledException) when (cts.IsCancellationRequested)
                                    {
                                        command.SetException(
                                            new ResumeOnStoppedTimeoutException(response, "Failed to wait for ResumedResponse during timespan"));
                                        isHandlingSuccessful = false;
                                    }
                                }
                            }
                        }
                        if (isHandlingSuccessful)
                        {
                            command.SetResult(response);
                        }
                    }
                    else if (task == dataAvailableTask)
                    {
                        // when awaited dataAvailableTask expect data to be present
                        // otherwise treat is as socket disconnected
                        if (socket.Available == 0)
                        {
                            throw new SocketDisconnectedException("Socket disconnected based on no data received");
                        }
                    }
                    while (socket.Available > 0)
                    {
                        await ProcessUnboundAsync(socket, ct);
                    }
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        internal async Task WaitForResumedResponse(Socket socket, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                while (socket.Available > 0)
                {
                    if ((await ProcessUnboundAsync(socket, ct)) is ResumedResponse)
                    {
                        return;
                    }
                }
                await socket.WaitForDataAsync(ct);
            }
        }

        internal async Task<ViceResponse> ProcessUnboundAsync(Socket socket, CancellationToken ct)
        {
            PerformanceProfiler.Add(new DataAvailableEvent(PerformanceDataType.Response, PerformanceProfiler.Ticks));
            _logger.LogDebug("Will process unbound response");
            var (response, _) = await GetResponseAsync(socket, ct).ConfigureAwait(false);
            PerformanceProfiler.Add(new ResponseReadEvent(response.GetType(), IsNested: false, PerformanceProfiler.Ticks));
            MessagesHistory.AddsResponseOnly(response);
            OnViceResponse(new ViceResponseEventArgs(response));
            return response;
        }

        private async Task<(ViceResponse Response, LastStatusResponse LastStatusResponse)> SendCommandAndWaitForResponse(Socket socket, IViceCommand command, CancellationToken ct)
        {
            _logger.LogDebug($"Will process command {_currentRequestId} of {command.GetType().Name} with request id {_currentRequestId}");
            await SendCommandAsync(socket, _currentRequestId, command, ct).ConfigureAwait(false);
            PerformanceProfiler.Add(new CommandSentEvent(command.GetType(), PerformanceProfiler.Ticks));
            int id = await MessagesHistory.AddCommandAsync(_currentRequestId, command);
            (command as IDisposable)?.Dispose();
            ViceResponse response;
            LastStatusResponse lastStatusResponse;
            switch (command)
            {
                // list is a special case that collects matching CheckpointInfoResponses
                case CheckpointListCommand:
                    var info = ImmutableArray<CheckpointInfoResponse>.Empty;
                    while ((
                        (response, lastStatusResponse) = await WaitUntilMatchesResponseAsync(socket, _currentRequestId, ct).ConfigureAwait(false))
                            .response is CheckpointInfoResponse infoResponse)
                    {
                        info = info.Add(infoResponse);
                        MessagesHistory.UpdateWithLinkedResponse(id, infoResponse);
                    }
                    var listResponse = (CheckpointListResponse)response;
                    response = listResponse with { Info = info };
                    break;
                default:
                    (response, lastStatusResponse) = await WaitUntilMatchesResponseAsync(socket, _currentRequestId, ct).ConfigureAwait(false);
                    _logger.LogDebug($"Command {_currentRequestId} of {command.GetType().Name} got response with result {response.ErrorCode}");
                    break;
            }
            _currentRequestId++;
            PerformanceProfiler.Add(new CommandCompletedEvent(command.GetType(), PerformanceProfiler.Ticks));
            MessagesHistory.UpdateWithResponse(id, response);
            return (response, lastStatusResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ct"></param>
        /// <returns>An instance of <see cref="ManagedBuffer"/> that has to be disposed after use.</returns>
        async Task<(ViceResponse Response, uint RequestId)> GetResponseAsync(Socket socket, CancellationToken ct)
        {
            using (var headerBuffer = _byteArrayPool.GetBuffer(12))
            {
                await ReadByteArrayAsync(socket, headerBuffer, ct).ConfigureAwait(false);
                uint responseBodyLength = _responseBuilder.GetResponseBodyLength(headerBuffer.Data.AsSpan());
                _logger.LogDebug($"Response body length is {responseBodyLength:#,##0}B");
                (ViceResponse Response, uint RequestId) result;
                if (responseBodyLength > 0)
                {
                    using (var bodyBuffer = _byteArrayPool.GetBuffer(responseBodyLength))
                    {
                        await ReadByteArrayAsync(socket, bodyBuffer, ct);
                        result = _responseBuilder.Build(headerBuffer.Data.AsSpan(), ViceCommand.DefaultApiVersion, 
                            bodyBuffer.Data.AsSpan()[0..(int)responseBodyLength]);
                    }
                }
                else
                {
                    result = _responseBuilder.Build(headerBuffer.Data.AsSpan(), ViceCommand.DefaultApiVersion, Array.Empty<byte>());
                }
                _logger.LogDebug($"Response is {result.Response.GetType().Name} with RequestId {result.RequestId}");
                return result;
            }
        }
        async Task SendCommandAsync(Socket socket, uint requestId, IViceCommand command, CancellationToken ct)
        {
            _logger.LogDebug($"Sending command {command.GetType().Name} with RequestId {requestId}");

            var (buffer, length) = command.GetBinaryData(requestId);
            try
            {
                _logger.LogDebug($"Sending command length is {length}");
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
                i += await socket.ReceiveAsync(dataSpan[i..(int)buffer.Size], SocketFlags.None, ct).ConfigureAwait(false);
            }
            while (i < buffer.Size);
        }
        async Task SendByteArrayAsync(Socket socket, byte[] data, int length, CancellationToken ct)
        {
            int i = 0;
            var dataSpan = data.AsMemory();
            do
            {
                int passes = 0;
                int delays = 0;
                int sent = await socket.SendAsync(dataSpan[i..length], SocketFlags.None, ct);
                if (sent > 0)
                {
                    passes++;
                    i += sent;
                }
                else
                {
                    delays++;
                    await Task.Delay(10, ct);
                }
                if (PerformanceProfiler.IsEnabled)
                {
                    PerformanceProfiler.Add(new RawSendEvent(passes, delays, PerformanceProfiler.Ticks));
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
            if (_cts is not null && _loop is not null)
            {
                _logger.LogDebug("Dispose async");
                await _cts.CancelAsync();
                try
                {
                    await _loop;
                }
                catch (OperationCanceledException)
                { }
                _logger.LogDebug("Disposed async");
                lock (_sync)
                {
                    _cts = null;
                    _loop = null;
                }
            }
            else
            {
                _logger.LogDebug("Nothing to dispose async");
            }
        }
        /// <inheritdoc/>
        public Task<bool> WaitForConnectionStatusChangeAsync(CancellationToken ct = default)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<ConnectedChangedEventArgs> handler = null!;
            handler = (_, e) =>
            {
                tcs.TrySetResult(e.IsConnected);
                ConnectedChanged -= handler;
            };
            ConnectedChanged += handler;
            ct.Register(() =>
            {
                tcs.TrySetCanceled();
                ConnectedChanged -= handler;
            });
            return tcs.Task;
        }
        /// <summary>
        /// Releases all resources used by the <see cref="ViceBridge"/>.
        /// </summary>
        public void Dispose()
        {
            _logger.LogDebug("Dispose");
            _cts?.Cancel();
        }
    }
}

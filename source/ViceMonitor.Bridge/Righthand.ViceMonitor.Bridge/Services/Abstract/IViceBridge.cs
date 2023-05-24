using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Implementation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Righthand.ViceMonitor.Bridge.Services.Abstract
{
    /// <summary>
    /// Interface to access <see cref="ViceBridge"/>
    /// </summary>
    public interface IViceBridge: IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Task where loop is running.
        /// </summary>
        Task? RunnerTask { get; }
        /// <summary>
        /// Gets running status. Running is set once the connection to VICE is established.
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// Gets started status. Started is set as soon as bridge starts.
        /// </summary>
        bool IsStarted { get; }
        /// <summary>
        /// Gets connection to VICE status.
        /// </summary>
        bool IsConnected { get; }
        IPerformanceProfiler PerformanceProfiler { get; }
        /// <summary>
        /// Starts the bridge.
        /// </summary>
        /// <param name="port">Port of the binary monitor. 6502 by default.</param>
        void Start(int port = 6502);
        /// <summary>
        /// Stops the bridge.
        /// </summary>
        /// <param name="waitForQueueToProcess">
        /// When true, bridge will process all commands in queue and then exit, cancel processing otherwise.
        /// </param>
        /// <returns></returns>
        Task StopAsync(bool waitForQueueToProcess);
        /// <summary>
        /// Enqueues command for sending
        /// </summary>
        /// <typeparam name="T">Command type</typeparam>
        /// <param name="command">An instance of <see cref="ViceCommand{TResponse}"/> subtype to enqueue.</param>
        /// <param name="resumeOnStopped">When true, ExitCommand is sent if <see cref="StoppedResponse"/> is received
        /// during command execution.</param>
        /// <returns>An instance of passed in command.</returns>
        T EnqueueCommand<T>(T command, bool resumeOnStopped = false)
            where T : IViceCommand;
        /// <summary>
        /// Occurs when an unbound event arrived.
        /// </summary>
        /// <threadsafety>Can occur on any thread.</threadsafety>
        event EventHandler<ViceResponseEventArgs>? ViceResponse;
        /// <summary>
        /// Occurs when connection status to VICE changes.
        /// </summary>
        /// <threadsafety>Can occur on any thread.</threadsafety>
        event EventHandler<ConnectedChangedEventArgs>? ConnectedChanged;
        /// <summary>
        /// Waits for connection status change.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>New IsConnectedValue.</returns>
        Task<bool> WaitForConnectionStatusChangeAsync(CancellationToken ct = default);
    }
}

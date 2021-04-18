using System;
using System.Net;
using System.Threading.Tasks;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Services.Implementation;

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
        /// <summary>
        /// Starts the bridge.
        /// </summary>
        /// <param name="port">Port of the binary monitor. 6502 by default.</param>
        void Start(int port = 6502);
        /// <summary>
        /// Stops the bridge.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
        /// <summary>
        /// Enqueues command for sending
        /// </summary>
        /// <typeparam name="T">Command type</typeparam>
        /// <param name="command">An instance of <see cref="ViceCommand{TResponse}"/> subtype to enqueue.</param>
        /// <returns>An instance of passed in command.</returns>
        T EnqueueCommand<T>(T command)
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
    }
}

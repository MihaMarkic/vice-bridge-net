namespace Righthand.ViceMonitor.Bridge.Services.Abstract
{
    /// <summary>
    /// Provides performance data logging.
    /// </summary>
    public interface IPerformanceProfiler
    {
        /// <summary>
        /// When disabled, it won't do anything.
        /// </summary>
        bool IsEnabled { get; }
        /// <summary>
        /// List of collected events.
        /// </summary>
        IReadOnlyList<PerformanceEvent> Events { get; }
        /// <summary>
        /// Ticks since creation or <see cref="Clear"/> method call.
        /// </summary>
        long Ticks { get; }
        /// <summary>
        /// Adds a performance event.
        /// </summary>
        /// <param name="e"></param>
        void Add(PerformanceEvent e);
        /// <summary>
        /// Clears log and resets start ticks.
        /// </summary>
        void Clear();
    }
    /// <summary>
    /// Type of data type that is available.
    /// </summary>
    public enum PerformanceDataType
    {
        /// <summary>
        /// VICE command to be sent.
        /// </summary>
        Command,
        /// <summary>
        /// VICE response to be received.
        /// </summary>
        Response
    }
    /// <summary>
    /// Superclass for event data tracking.
    /// </summary>
    /// <param name="Ticks"></param>
    public abstract record PerformanceEvent(long Ticks);
    /// <summary>
    /// Bridge started listening for either command or an incoming response.
    /// </summary>
    /// <param name="Ticks"></param>
    public record StartListeningEvent(long Ticks) : PerformanceEvent(Ticks);
    /// <summary>
    /// Data is available. Happens after <see cref="StartListeningEvent"/>.
    /// </summary>
    /// <param name="DataType"></param>
    /// <param name="Ticks"></param>
    public record DataAvailableEvent(PerformanceDataType DataType, long Ticks) : PerformanceEvent(Ticks);
    /// <summary>
    /// Command has been sent.
    /// </summary>
    /// <param name="CommandType"></param>
    /// <param name="Ticks"></param>
    public record CommandSentEvent(Type CommandType, long Ticks) : PerformanceEvent(Ticks);
    /// <summary>
    /// Commands has received response and all unbound responses between have been processed.
    /// </summary>
    /// <param name="CommandType"></param>
    /// <param name="Ticks"></param>
    public record CommandCompletedEvent(Type CommandType, long Ticks) : PerformanceEvent(Ticks);
    /// <summary>
    /// Response has been read.
    /// </summary>
    /// <param name="ResponseType"></param>
    /// <param name="IsNested"></param>
    /// <param name="Ticks"></param>
    public record ResponseReadEvent(Type ResponseType, bool IsNested, long Ticks) : PerformanceEvent(Ticks);
    /// <summary>
    /// Raw command sending data.
    /// </summary>
    /// <param name="Passes"></param>
    /// <param name="Delays">Happens if no bytes have been written in a pass.</param>
    /// <param name="Ticks"></param>
    public record RawSendEvent(int Passes, int Delays, long Ticks) : PerformanceEvent(Ticks);
    /// <summary>
    /// Generic trace event.
    /// </summary>
    /// <param name="Info"></param>
    /// <param name="Ticks"></param>
    public  record TraceEvent(string Info, long Ticks) : PerformanceEvent(Ticks);
}

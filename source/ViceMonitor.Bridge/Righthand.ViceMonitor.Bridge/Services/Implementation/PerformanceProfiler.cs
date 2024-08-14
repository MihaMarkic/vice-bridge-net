using Righthand.ViceMonitor.Bridge.Services.Abstract;
using System.Diagnostics;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation
{
    /// <inheritdoc/>
    /// <threadsafety>Thread safe.</threadsafety>
    public class PerformanceProfiler : IPerformanceProfiler
    {
        private readonly object _sync = new();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly List<PerformanceEvent> _events = new();
        /// <inheritdoc/>
        public long Ticks => _stopwatch.ElapsedMilliseconds;
        /// <inheritdoc/>
        public ImmutableArray<PerformanceEvent> Events
        {
            get
            {
                lock (_sync)
                {
                    return [.._events];
                }
            }
        }
        /// <inheritdoc/>
        public bool IsEnabled => true;
        /// <inheritdoc/>
        public void Add(PerformanceEvent e)
        {
            lock (_sync)
            {
                _events.Add(e);
            }
        }
        /// <inheritdoc/>
        public void Clear()
        {
            lock (_sync)
            {
                _events.Clear();
                _stopwatch.Restart();
            }
        }
    }
}

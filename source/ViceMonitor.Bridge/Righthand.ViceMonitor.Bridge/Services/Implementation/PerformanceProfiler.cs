using Righthand.ViceMonitor.Bridge.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation
{
    /// <inheritdoc/>
    /// <threadsafety>Thread safe.</threadsafety>
    public class PerformanceProfiler : IPerformanceProfiler
    {
        readonly object sync = new object();
        /// <inheritdoc/>
        readonly Stopwatch stopwatch = Stopwatch.StartNew();
        /// <inheritdoc/>
        readonly List<PerformanceEvent> events = new List<PerformanceEvent>();
        /// <inheritdoc/>
        public long Ticks => stopwatch.ElapsedMilliseconds;
        /// <inheritdoc/>
        public IReadOnlyList<PerformanceEvent> Events
        {
            get
            {
                lock (sync)
                {
                    return events.ToImmutableArray();
                }
            }
        }
        /// <inheritdoc/>
        public bool IsEnabled => true;
        /// <inheritdoc/>
        public void Add(PerformanceEvent e)
        {
            lock (sync)
            {
                events.Add(e);
            }
        }
        /// <inheritdoc/>
        public void Clear()
        {
            lock (sync)
            {
                events.Clear();
                stopwatch.Restart();
            }
        }
    }
}

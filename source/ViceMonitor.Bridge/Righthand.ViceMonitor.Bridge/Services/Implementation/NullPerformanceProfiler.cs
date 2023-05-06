using System;
using System.Collections.Generic;
using Righthand.ViceMonitor.Bridge.Services.Abstract;

namespace Righthand.ViceMonitor.Bridge.Services.Implementation
{
    /// <summary>
    /// Non logging performance profiler. Typically used in Relase version.
    /// </summary>
    /// <threadsafety>Thread safe.</threadsafety>
    internal class NullPerformanceProfiler : IPerformanceProfiler
    {
        /// <inheritdoc/>
        public bool IsEnabled => false;
        /// <inheritdoc/>
        public long Ticks => 0;
        /// <inheritdoc/>
        public IReadOnlyList<PerformanceEvent> Events => Array.Empty<PerformanceEvent>();
        /// <inheritdoc/>
        public void Add(PerformanceEvent e)
        { }
        /// <inheritdoc/>
        public void Clear()
        { }
    }
}

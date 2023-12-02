using Microsoft.Extensions.DependencyInjection;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Righthand.ViceMonitor.Bridge.Services.Implementation;

namespace Righthand.ViceMonitor.Bridge
{
    /// <summary>
    /// Provides IoC types registration.
    /// </summary>
    public static class IoC
    {
        /// <summary>
        /// Adds required registrations.
        /// </summary>
        /// <param name="services"></param>
        public static void AddViceBridge(this IServiceCollection services)
        {
            services.AddSingleton<IViceBridge, ViceBridge>();
#if DEBUG
            services.AddSingleton<IPerformanceProfiler, PerformanceProfiler>();
            services.AddSingleton<IMessagesHistory, NullMessagesHistory>();
#else
            services.AddSingleton<IPerformanceProfiler, NullPerformanceProfiler>();
            services.AddSingleton<IMessagesHistory, NullMessagesHistory>();
#endif
            services.AddSingleton<ResponseBuilder>();
        }
    }
}

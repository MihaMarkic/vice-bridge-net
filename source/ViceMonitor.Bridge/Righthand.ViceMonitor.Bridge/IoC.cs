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
            services.AddSingleton<ResponseBuilder>();
        }
    }
}

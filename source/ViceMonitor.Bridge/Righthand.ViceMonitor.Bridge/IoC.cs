using Microsoft.Extensions.DependencyInjection;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Righthand.ViceMonitor.Bridge.Services.Implementation;

namespace Righthand.ViceMonitor.Bridge
{
    public static class IoC
    {
        public static void AddEngineServices(this IServiceCollection services)
        {
            services.AddSingleton<IViceBridge, ViceBridge>();
            services.AddSingleton<ResponseBuilder>();
        }
    }
}

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge;
using NLog.Extensions.Logging;

namespace Righthand.ViceMonitor.Bridge.Trace
{
    public static class ContainerConfiguration
    {
        public static IServiceCollection ConfigureServices()
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .Build();

            var collection = new ServiceCollection();
            collection.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddNLog(config);
            });
            collection.AddViceBridge();
            return collection;
        }
    }
}

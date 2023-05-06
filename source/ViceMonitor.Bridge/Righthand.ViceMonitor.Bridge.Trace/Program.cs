using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Righthand.ViceMonitor.Bridge.Trace;
using Spectre.Console;

namespace Righthand.ViceMonitor.Bridge.Trace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            NLog.Common.InternalLogger.LogToConsole = true;
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                AnsiConsole.WriteLine("Initializing");
                var services = ContainerConfiguration.ConfigureServices();
                services.AddSingleton<Application>();
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    var application = serviceProvider.GetService<Application>()!;
                    var cts = new CancellationTokenSource();
                    await application.RunAsync(cts.Token);
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of an exception");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

    }
}

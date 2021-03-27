using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Spectre.Console;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModernVICEPDBMonitor.Playground
{
    public class Application
    {
        readonly ILogger logger;
        readonly IViceBridge bridge;
        public Application(ILogger<Application> logger, IViceBridge bridge)
        {
            this.logger = logger;
            this.bridge = bridge;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            try
            {
                UpdateConnectedState();
                var command = new CheckpointSetCommand(0xfce2, 0xfce3,
                    StopWhenHit: true, Enabled: true, CpuOperation.Exec, Temporary: true);
                bridge.EnqueCommand(command);
                bridge.Start(IPAddress.Loopback);
                bridge.ConnectedChanged += Bridge_ConnectedChanged;
                AnsiConsole.MarkupLine("Waiting for command response");
                var response = await command.Result;
                AnsiConsole.MarkupLine($"Got response [bold]{response.GetType().Name}[/]");
                AnsiConsole.WriteLine("Press ENTER to end");
                Console.ReadLine();
                await bridge.DisposeAsync();
                AnsiConsole.WriteLine("Main loop was cancelled");
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.WriteLine("Main loop was cancelled");
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                logger.LogError(ex, "Main loop failure");
            }
            Console.WriteLine("App stopped");
        }

        void Bridge_ConnectedChanged(object sender, ConnectedChangedEventArgs e)
        {
            UpdateConnectedState();
        }
        void UpdateConnectedState()
        {
            AnsiConsole.MarkupLine($"Bridge is {(bridge.IsConnected ? "[green]connected[/]": "[red]disconnected[/]")}");
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Spectre.Console;
using System;
using System.IO;
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
                ct.ThrowIfCancellationRequested();
                UpdateConnectedState();
                //var checkPointCommand = new CheckpointSetCommand(0xfce2, 0xfce3,
                //    StopWhenHit: true, Enabled: true, CpuOperation.Exec, Temporary: true);
                //bridge.EnqueCommand(checkPointCommand);
                bridge.Start(IPAddress.Loopback);
                bridge.ConnectedChanged += Bridge_ConnectedChanged;
                await GetDisplayAsync(ct);
                AnsiConsole.MarkupLine("Waiting for command response");
                AnsiConsole.WriteLine("Press ENTER to end");
                Console.ReadLine();
                await bridge.DisposeAsync();
                AnsiConsole.WriteLine("Main loop was canceled");
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.WriteLine("Main loop was canceled");
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                logger.LogError(ex, "Main loop failure");
            }
            Console.WriteLine("App stopped");
        }

        async Task GetDisplayAsync(CancellationToken ct)
        {
            var displayGetCommand = new DisplayGetCommand(UseVic: true, ImageFormat.Rgb);
                bridge.EnqueCommand(displayGetCommand);
            var response = await displayGetCommand.Result;
            AnsiConsole.MarkupLine($"Got response [bold]{response.GetType().Name}[/]");
            var image = response.Image.Value;
            AnsiConsole.MarkupLine($"Image size is [bold]{response.InnerWidth}[/]x[bold]{response.InnerHeight}[/]");
            string tempFileName = Path.Combine(Path.GetTempPath(), "test.raw");
            using (var stream = File.OpenWrite(tempFileName))
            {
                stream.Write(image.Data, 0, (int)image.Size);
                await stream.FlushAsync();
            }
            response.Dispose();
            AnsiConsole.MarkupLine($"Image written to [bold]{tempFileName}[/]");
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

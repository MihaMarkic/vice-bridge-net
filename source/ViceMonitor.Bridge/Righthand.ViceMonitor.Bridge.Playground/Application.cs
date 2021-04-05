using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Spectre.Console;
using System;
using System.Collections.Immutable;
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
                bridge.Start();
                bridge.ConnectedChanged += Bridge_ConnectedChanged;
                try
                {
                    await ShowMenuAsync(ct);
                }
                finally
                {
                    await bridge.DisposeAsync();
                    AnsiConsole.WriteLine("Main loop was canceled");
                }
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

        async Task ShowMenuAsync(CancellationToken ct)
        {
            var options = ImmutableDictionary<string, string>.Empty
                .Add("dg", "Display get")
                .Add("vi", "VICE info")
                .Add("cl", "Checkpoint list")
                .Add("cs", "Checkpoint set");
            bool quit = false;
            while (!quit)
            {
                foreach (var pair in options)
                {
                    AnsiConsole.MarkupLine($"[bold]{pair.Key}[/]  ... {pair.Value}");
                }
                AnsiConsole.MarkupLine("Type [bold]q[/] to end");
                string? command = Console.ReadLine();
                switch (command)
                {
                    case "dg":
                        await GetDisplayAsync(ct);
                        break;
                    case "vi":
                        await ViceInfoAsync(ct);
                        break;
                    case "cl":
                        await CheckpointListAsync(ct);
                        break;
                    case "cs":
                        await CheckpointSetAsync(ct);
                        break;
                    case "q":
                        quit = true;
                        break;
                }
            }
        }
        async Task CheckpointSetAsync(CancellationToken ct)
        {
            var setCommand = bridge.EnqueueCommand(new CheckpointSetCommand(0x1000, 0x2000, StopWhenHit: true, Enabled: true,
               CpuOperation: CpuOperation.Load, Temporary: true));
            var setResponse = await setCommand.Response;
            AnsiConsole.MarkupLine($"Set response: {setResponse.ErrorCode}");
        }
        async Task CheckpointListAsync(CancellationToken ct)
        {
            var listCommand = new CheckpointListCommand();
            bridge.EnqueueCommand(listCommand);
            var listResponse = await listCommand.Response;
            AnsiConsole.MarkupLine($"Response has [bold]{listResponse.TotalNumberOfCheckpoints}[/] total number of checkpoints.");
            AnsiConsole.MarkupLine("List");
            int index = 0;
            foreach (var info in listResponse.Info)
            {
                AnsiConsole.MarkupLine($"[bold]{index}[/]: Number:{info.CheckpointNumber} CpuOperation:{info.CpuOperation}");
                index++;
            }
        }
        async Task ViceInfoAsync(CancellationToken ct)
        {
            var command = new InfoCommand();
            bridge.EnqueueCommand(command);
            var response = await command.Response;
            AnsiConsole.MarkupLine($"VICE version RC number is [bold]{response.VersionRCNumber}[/]");

        }
        async Task GetDisplayAsync(CancellationToken ct)
        {
            var command = new DisplayGetCommand(UseVic: true, ImageFormat.Rgb);
            bridge.EnqueueCommand(command);
            var response = await command.Response;
            try
            {
                AnsiConsole.MarkupLine($"Got response [bold]{response.GetType().Name}[/]");
                if (response.Image is not null)
                {
                    var image = response.Image.Value;
                    AnsiConsole.MarkupLine($"Image size is [bold]{response.InnerWidth}[/]x[bold]{response.InnerHeight}[/]");
                    string tempFileName = Path.Combine(Path.GetTempPath(), "test.raw");
                    using (var stream = File.OpenWrite(tempFileName))
                    {
                        stream.Write(image.Data, 0, (int)image.Size);
                        await stream.FlushAsync(ct);
                    }
                    AnsiConsole.MarkupLine($"Image written to [bold]{tempFileName}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("Response does not contain image data");
                }
            }
            finally
            {
                response.Dispose();
            }
        }

        void Bridge_ConnectedChanged(object? sender, ConnectedChangedEventArgs e)
        {
            UpdateConnectedState();
        }
        void UpdateConnectedState()
        {
            AnsiConsole.MarkupLine($"Bridge is {(bridge.IsConnected ? "[green]connected[/]": "[red]disconnected[/]")}");
        }
    }
}

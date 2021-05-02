using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog.LayoutRenderers.Wrappers;
using Righthand.ViceMonitor.Bridge;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Righthand.ViceMonitor.Bridge.Shared;
using Spectre.Console;

namespace ModernVICEPDBMonitor.Playground
{
    public class Application
    {
        readonly ILogger logger;
        readonly IViceBridge bridge;
        ImmutableDictionary<byte, FullRegisterItem> registers;
        public Application(ILogger<Application> logger, IViceBridge bridge)
        {
            this.logger = logger;
            this.bridge = bridge;
            registers = ImmutableDictionary<byte, FullRegisterItem>.Empty;
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
                bridge.ViceResponse += Bridge_ViceResponse;
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

        void Bridge_ViceResponse(object? sender, ViceResponseEventArgs e)
        {
            AnsiConsole.MarkupLine($"Got unbound [bold]{e.Response.GetType().Name}[/]");
            switch (e.Response)
            {
                case RegistersResponse registers:
                    OutputRegisters(registers.Items);
                    break;
            }
        }

        async Task ShowMenuAsync(CancellationToken ct)
        {
            var options = ImmutableArray<KeyValuePair<string, string>>.Empty
                .Add(new KeyValuePair<string, string>("dg", "Display get"))
                //.Add("vi", "VICE info")
                .Add(new KeyValuePair<string, string>("cl", "Checkpoint list"))
                .Add(new KeyValuePair<string, string>("cs", "Checkpoint set"))
                .Add(new KeyValuePair<string, string>("p", "Ping"))
                .Add(new KeyValuePair<string, string>("ra", "Registers available"))
                .Add(new KeyValuePair<string, string>("rg", "Registers get"))
                .Add(new KeyValuePair<string, string>("qv", "Quit VICE"))
                .Add(new KeyValuePair<string, string>("e", "Exit (resumes execution)"))
                .Add(new KeyValuePair<string, string>("start", "Start bridge"))
                .Add(new KeyValuePair<string, string>("stop", "Stop bridge"));
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
                    //case "vi":
                    //    await ViceInfoAsync(ct);
                    //    break;
                    case "cl":
                        await CheckpointListAsync(ct);
                        break;
                    case "cs":
                        await CheckpointSetAsync(ct);
                        break;
                    case "p":
                        await PingAsync(ct);
                        break;
                    case "e":
                        await ExitAsync(ct);
                        break;
                    case "ra":
                        await RegistersAvailableAsync(ct);
                        break;
                    case "rg":
                        await RegistersGetAsync(ct);
                        break;
                    case "qv":
                        await QuitViceAsync(ct);
                        break;
                    case "start":
                        bridge.Start();
                        break;
                    case "stop":
                        await StopBridgeAsync(ct);
                        break;
                    case "q":
                        quit = true;
                        break;
                }
            }
        }
        internal async Task<T?> AwaitWithTimeoutAsync<T>(Task<T> task, Action<T> textOnSuccess)
        {
            bool success = await Task.WhenAny(task, Task.Delay(5000)) == task;
            if (success)
            {
                textOnSuccess(task.Result);
                return task.Result;
            }
            AnsiConsole.MarkupLine("[red]Response timed out[/]");
            return default;
        }
        async Task StopBridgeAsync(CancellationToken ct)
        {
            var stopTask = bridge.StopAsync();
            bool result = (await Task.WhenAny(stopTask, Task.Delay(5000, ct)) == stopTask);
            if (!result)
            {
                AnsiConsole.MarkupLine("[bold]Failed stopping bridge[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("Bridge stopped");
            }
        }
        async Task RegistersAvailableAsync(CancellationToken ct)
        {
            var command = bridge.EnqueueCommand(new RegistersAvailableCommand(MemSpace.MainMemory));
            await AwaitWithTimeoutAsync(command.Response, response =>
            {
                string markup = string.Join("\n", response.Items.OrderBy(i => i.Id).Select(i => $"\t[bold]{i.Id}[/]:{i.Name} {i.Size}bytes"));
                AnsiConsole.MarkupLine($"Registers:\n{markup}");
                registers = response.Items.ToImmutableDictionary(i => i.Id, i => i);
            });
        }
        async Task RegistersGetAsync(CancellationToken ct)
        {
            var command = bridge.EnqueueCommand(new RegistersGetCommand(MemSpace.MainMemory));
            await AwaitWithTimeoutAsync(command.Response, response => OutputRegisters(response.Items));

        }
        void OutputRegisters(IList<RegisterItem> items)
        {
            string markup = string.Join(" ", items.Select(i => 
            {
                if (registers.TryGetValue(i.RegisterId, out var register))
                {
                    string value = register.Size switch
                    {
                        1 => i.RegisterValue > 0 ? "T" : "F",
                        _ => i.RegisterValue.ToString($"x{register.Size / 4}"),
                    };
                    return $"[bold]{register.Name }[/]:{value}";
                }
                else
                {
                    return $"[bold]{i.RegisterId}[/]:{i.RegisterValue:x2}";
                }
            }));
            AnsiConsole.MarkupLine($"Registers: {markup}");
        }
        async Task PingAsync(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var ping = bridge.EnqueueCommand(new PingCommand());
            await AwaitWithTimeoutAsync(ping.Response, response => AnsiConsole.MarkupLine($"Ping response: {response.ErrorCode} in {sw.ElapsedMilliseconds:#,##0}ms"));
        }
        async Task ExitAsync(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var ping = bridge.EnqueueCommand(new ExitCommand());
            await AwaitWithTimeoutAsync(ping.Response, response => AnsiConsole.MarkupLine($"Resume response: {response.ErrorCode} in {sw.ElapsedMilliseconds:#,##0}ms"));
        }
        async Task QuitViceAsync(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var ping = bridge.EnqueueCommand(new QuitCommand());
            await AwaitWithTimeoutAsync(ping.Response, response => AnsiConsole.MarkupLine($"Quit response: {response.ErrorCode} in {sw.ElapsedMilliseconds:#,##0}ms"));
        }
        async Task CheckpointSetAsync(CancellationToken ct)
        {
            var setCommand = bridge.EnqueueCommand(new CheckpointSetCommand(0x1000, 0x2000, StopWhenHit: true, Enabled: true,
               CpuOperation: CpuOperation.Load, Temporary: true));
            await AwaitWithTimeoutAsync(setCommand.Response, response => AnsiConsole.MarkupLine($"Set response: {response.ErrorCode}"));
        }
        async Task CheckpointListAsync(CancellationToken ct)
        {
            var listCommand = new CheckpointListCommand();
            bridge.EnqueueCommand(listCommand);
            Action<CheckpointListResponse> onSuccess = response =>
            {
                AnsiConsole.MarkupLine($"Response has [bold]{response.TotalNumberOfCheckpoints}[/] total number of checkpoints.");
                AnsiConsole.MarkupLine("List");
                int index = 0;
                foreach (var info in response.Info)
                {
                    AnsiConsole.MarkupLine($"[bold]{index}[/]: Number:{info.CheckpointNumber} CpuOperation:{info.CpuOperation}");
                    index++;
                }
            };
            await AwaitWithTimeoutAsync(listCommand.Response, onSuccess);
        }
        // not yet implemented in VICE stable
        //async Task ViceInfoAsync(CancellationToken ct)
        //{
        //    var command = new InfoCommand();
        //    bridge.EnqueueCommand(command);
        //    var response = await command.Response;
        //    AnsiConsole.MarkupLine($"VICE version RC number is [bold]{response.VersionRCNumber}[/]");

        //}
        async Task GetDisplayAsync(CancellationToken ct)
        {
            var command = new DisplayGetCommand(UseVic: true, ImageFormat.Rgb);
            bridge.EnqueueCommand(command);
            Action<DisplayGetResponse> onSuccess = async response =>
            {
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
            };
            await AwaitWithTimeoutAsync(command.Response, onSuccess);
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

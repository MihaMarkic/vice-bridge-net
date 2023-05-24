using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Righthand.ViceMonitor.Bridge.Shared;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernVICEPDBMonitor.Playground
{
    public class Application
    {
        readonly ILogger logger;
        readonly IViceBridge bridge;
        ImmutableDictionary<byte, FullRegisterItem> registers;
        uint? tracepointNumber;
        bool isViceStopped;
        readonly object sync = new();
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
                    bool run = true;
                    while (run)
                    {
                        try
                        {
                            await ShowMenuAsync(ct);
                            run = false;
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.WriteException(ex);
                        }
                    }
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
                case StoppedResponse:
                    lock (sync)
                    {
                        isViceStopped = true;
                    }
                    if (tracepointNumber.HasValue)
                    {
                        bridge.EnqueueCommand(new ExitCommand());
                    }
                    break;
                case ResumedResponse:
                    lock (sync)
                    {
                        isViceStopped = false;
                    }
                    break;
            }
        }

        async Task ShowMenuAsync(CancellationToken ct)
        {
            var options = ImmutableArray<KeyValuePair<string, string>>.Empty
                .Add(new KeyValuePair<string, string>("dg", "Display get"))
                .Add(new KeyValuePair<string, string>("vi", "VICE info"))
                .Add(new KeyValuePair<string, string>("cl", "Checkpoint list"))
                .Add(new KeyValuePair<string, string>("cs", "Checkpoint set"))
                .Add(new KeyValuePair<string, string>("cd", "Checkpoint delete"))
                .Add(new KeyValuePair<string, string>("ct", "Checkpoint toggle"))
                .Add(new KeyValuePair<string, string>("os", "Condition set"))
                .Add(new KeyValuePair<string, string>("mg", "Memory get"))
                .Add(new KeyValuePair<string, string>("ms", "Memory set"))
                .Add(new KeyValuePair<string, string>("p", "Ping"))
                .Add(new KeyValuePair<string, string>("ra", "Registers available"))
                .Add(new KeyValuePair<string, string>("rg", "Registers get"))
                .Add(new KeyValuePair<string, string>("rse", "Registers invalid set"))
                .Add(new KeyValuePair<string, string>("qv", "Quit VICE"))
                .Add(new KeyValuePair<string, string>("e", "Exit (resumes execution)"))
                .Add(new KeyValuePair<string, string>("start", "Start bridge"))
                .Add(new KeyValuePair<string, string>("stop", "Stop bridge"))
                .Add(new KeyValuePair<string, string>("l", "Loads sample tiny.o"))
                .Add(new KeyValuePair<string, string>("nc", "Nested call"))
                .Add(new KeyValuePair<string, string>("ros", "Resume On Stop"))
                .Add(new KeyValuePair<string, string>("s", "Starts loaded sample tiny.o"));
            bool quit = false;
            while (!quit)
            {
                foreach (var pair in options)
                {
                    AnsiConsole.MarkupLine($"[bold]{pair.Key}[/]  ... {pair.Value}");
                }
                string viceStatus = isViceStopped ? "[red]stopped[/]" : "[green]running[/]";
                AnsiConsole.MarkupLine($"Vice is {viceStatus}");
                AnsiConsole.MarkupLine("Type [bold]q[/] to end");
                string? command = Console.ReadLine();
                switch (command)
                {
                    case null:
                        break;
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
                    case "ts":
                        await TracepointSetAsync(ct);
                        break;
                    case "mg":
                        await MemoryGetAsync(ct);
                        break;
                    case "ms":
                        await MemorySetAsync(ct);
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
                    case "rse":
                        await RegistersSetAsync(ct, new RegisterItem(byte.MaxValue, ushort.MaxValue));
                        break;
                    case "qv":
                        await QuitViceAsync(ct);
                        break;
                    case "start":
                        bridge.Start();
                        break;
                    case "l":
                        await LoadSampleAsync(ct);
                        break;
                    case "s":
                        await StartSampleAsync(ct);
                        break;
                    case "nc":
                        await NestedCallAsync(ct);
                        break;
                    case "ros":
                        await ResumeOnStopAsync(ct);
                        break;
                    case "stop":
                        await StopBridgeAsync(waitForQueueToProcess: false, ct);
                        break;
                    case "q":
                        quit = true;
                        break;
                    default:
                        var parts = command.Split(' ').Where(p => !string.IsNullOrWhiteSpace(p)).ToImmutableArray();
                        if (parts.Length > 1)
                        {
                            switch (parts[0])
                            {
                                case "cd":
                                    {
                                        if (uint.TryParse(parts[1], out var checkpointNumber))
                                        {
                                            await CheckpointDeleteAsync(checkpointNumber, ct);
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("Expected an [red]uint[/] as an checkpoint number argument");
                                        }
                                    }
                                    break;
                                case "os":
                                    {
                                        if (uint.TryParse(parts[1], out var checkpointNumber))
                                        {
                                            string? condition = parts.Length > 2 ? parts[2] : null;
                                            await ConditionSetAsync(checkpointNumber, condition ?? "A == $0", ct);
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("Expected an [red]uint[/] as an checkpoint number argument and optionally condition text");
                                        }
                                    }
                                    break;
                                case "ct":
                                    {
                                        if (uint.TryParse(parts[1], out var checkpointNumber))
                                        {
                                            bool enabled;
                                            if (!(parts.Length > 2 && bool.TryParse(parts[2], out enabled)))
                                            {
                                                enabled = false;
                                            }
                                            AnsiConsole.MarkupLine($"Setting enabled to [bold]{enabled}[/]");
                                            await CheckpointToggleAsync(checkpointNumber, enabled, ct);
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine("Expected an [red]uint[/] as an checkpoint number argument and optionally condition text");
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        internal async Task ResumeOnStopAsync(CancellationToken ct)
        {
            //var command = bridge.EnqueueCommand(new RegistersGetCommand(MemSpace.MainMemory), resumeOnStopped: true);
            var command = bridge.EnqueueCommand(new CheckpointSetCommand(2214, 2213, StopWhenHit: true, Enabled: true,
                CpuOperation: CpuOperation.Load, Temporary: false), true);
            await AwaitWithTimeoutAsync(command.Response, cr => {
                string viceStatus;
                lock (sync)
                {
                    viceStatus = isViceStopped ? "[red]stopped[/]" : "[green]running[/]";
                }
                AnsiConsole.MarkupLine($"Got registers, status is {viceStatus}, should resume VICE"); 
            });
        }
        internal async Task NestedCallAsync(CancellationToken ct)
        {
            var listCommand = new CheckpointListCommand();
            bridge.EnqueueCommand(listCommand);
            bool isRunning = true;
            EventHandler<ViceResponseEventArgs> response = (sender, r) =>
            {
                switch (r.Response)
                {
                    case RegistersResponse:
                        AnsiConsole.MarkupLine("Got nested RegistersResponse, enqueuing RegistersAvailableCommand");
                        var availableRegistersCommand = new RegistersAvailableCommand(MemSpace.MainMemory);
                        bridge.EnqueueCommand(availableRegistersCommand);
                        _ = AwaitWithTimeoutAsync(availableRegistersCommand.Response, r =>
                        {
                            AnsiConsole.MarkupLine("Got RegistersAvailableCommand response");
                            if (!isRunning)
                            {
                                string status = isRunning ? "[green]running[/]" : "[red]stopped[/]";
                                AnsiConsole.MarkupLine($"Nested [yellow]resuming[/] on {status}");
                                bridge.EnqueueCommand(new ExitCommand());
                            }
                        });
                        break;
                    case StoppedResponse:
                        isRunning = false;
                        AnsiConsole.MarkupLine("[red]stopped[/]");
                        break;
                    case ResumedResponse:
                        isRunning = true;
                        AnsiConsole.MarkupLine("[green]resumed[/]");
                        break;
                }
            };
            bridge.ViceResponse += response;
            await AwaitWithTimeoutAsync(listCommand.Response, r => 
            {
                bridge.ViceResponse -= response;
                AnsiConsole.MarkupLine("Done");
                if (!isRunning)
                {
                    string status = isRunning ? "[green]running[/]" : "[red]stopped[/]";
                    AnsiConsole.MarkupLine($"Root [yellow]resuming[/] on {status}");
                    bridge.EnqueueCommand(new ExitCommand());
                }
            });
        }

        internal async Task<CommandResponse<TResponse>?> AwaitWithTimeoutAsync<TResponse>(Task<CommandResponse<TResponse>> task, Action<CommandResponse<TResponse>> textOnSuccess)
            where TResponse: ViceResponse
        {
            bool success = await Task.WhenAny(task, Task.Delay(5000)) == task;
            if (success)
            {
                var commandResponse = task.Result;
                if (commandResponse.IsSuccess)
                {
                    textOnSuccess(task.Result);
                }
                else
                {
                    AnsiConsole.MarkupLine($"Response returned an error [red]{commandResponse.ErrorCode}[/]");
                }
                return commandResponse;
            }
            AnsiConsole.MarkupLine("[red]Response timed out[/]");
            return default;
        }
        async Task StopBridgeAsync(bool waitForQueueToProcess, CancellationToken ct)
        {
            var stopTask = bridge.StopAsync(waitForQueueToProcess);
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
            await AwaitWithTimeoutAsync(command.Response, cr =>
            {
                var response = cr.Response!;
                string markup = string.Join("\n", response.Items.OrderBy(i => i.Id).Select(i => $"\t[bold]{i.Id}[/]:{i.Name} {i.Size}bytes"));
                AnsiConsole.MarkupLine($"Registers:\n{markup}");
                registers = response.Items.ToImmutableDictionary(i => i.Id, i => i);
            });
        }
        async Task StartSampleAsync(CancellationToken ct)
        {
            var register = registers.Values.Single(r => r.Name == "PC");
            var registerItem = new RegisterItem(register.Id, 0xC000);
            var argument = ImmutableArray<RegisterItem>.Empty.Add(registerItem);
            var command = bridge.EnqueueCommand(new RegistersSetCommand(MemSpace.MainMemory, argument));
            await AwaitWithTimeoutAsync(command.Response, cr => OutputRegisters(cr.Response!.Items));

        }
        async Task RegistersGetAsync(CancellationToken ct)
        {
            var command = bridge.EnqueueCommand(new RegistersGetCommand(MemSpace.MainMemory));
            await AwaitWithTimeoutAsync(command.Response, cr => OutputRegisters(cr.Response!.Items));
        }
        async Task RegistersSetAsync(CancellationToken ct, params RegisterItem[] args)
        {
            var command = bridge.EnqueueCommand(new RegistersSetCommand(MemSpace.MainMemory, args));
            await AwaitWithTimeoutAsync(command.Response, cr => OutputRegisters(cr.Response!.Items));
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
        async Task LoadSampleAsync(CancellationToken ct)
        {
            var file = Path.Combine(Path.GetDirectoryName(typeof(Application).Assembly.Location)!, "Samples", "tiny.o");
            var command = bridge.EnqueueCommand(new AutoStartCommand(runAfterLoading: false, 0, file));
            var response = await command.Response;
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
            await AwaitWithTimeoutAsync(setCommand.Response, response => 
                AnsiConsole.MarkupLine($"Checkpoint set response: {response.ErrorCode} with Checkpoint Number {response.Response?.CheckpointNumber}"));
        }
        async Task TracepointSetAsync(CancellationToken ct)
        {
            var setCommand = bridge.EnqueueCommand(new CheckpointSetCommand(0xd4fc, 0xd4fc, StopWhenHit: true, Enabled: true,
               CpuOperation: CpuOperation.Store, Temporary: false));
            await AwaitWithTimeoutAsync(setCommand.Response, response =>
            {
                tracepointNumber = response.Response?.CheckpointNumber;
                AnsiConsole.MarkupLine($"Tracepoint set response: {response.ErrorCode} with Checkpoint Number {response.Response?.CheckpointNumber}");
            });
        }
        async Task CheckpointDeleteAsync(uint checkpointNumber, CancellationToken ct)
        {
            var setCommand = bridge.EnqueueCommand(new CheckpointDeleteCommand(checkpointNumber));
            await AwaitWithTimeoutAsync(setCommand.Response, response => 
                AnsiConsole.MarkupLine($"Set response: {response.ErrorCode} and response type {response.Response?.GetType().Name}"));
        }
        async Task CheckpointToggleAsync(uint checkpointNumber, bool enabled, CancellationToken ct)
        {
            var setCommand = bridge.EnqueueCommand(new CheckpointToggleCommand(checkpointNumber, enabled));
            await AwaitWithTimeoutAsync(setCommand.Response, response =>
                AnsiConsole.MarkupLine($"Set response: {response.ErrorCode} and response type {response.Response?.GetType().Name}"));
        }
        async Task CheckpointListAsync(CancellationToken ct)
        {
            var listCommand = new CheckpointListCommand();
            bridge.EnqueueCommand(listCommand);
            Action<CommandResponse<CheckpointListResponse>> onSuccess = cr =>
            {
                var response = cr.Response;
                AnsiConsole.MarkupLine($"Response has [bold]{response!.TotalNumberOfCheckpoints}[/] total number of checkpoints.");
                AnsiConsole.MarkupLine("List");
                int index = 0;
                foreach (var info in response.Info)
                {
                    string status = info.Enabled ? "[bold]enabled[/]" : "[red]disabled[/]";
                    AnsiConsole.MarkupLine($"[bold]{index}[/]: Number:{info.CheckpointNumber} CpuOperation:{info.CpuOperation} {status}");
                    index++;
                }
            };
            await AwaitWithTimeoutAsync(listCommand.Response, onSuccess);
        }
        async Task ConditionSetAsync(uint checkpointNumber, string condition, CancellationToken ct)
        {
            var command = bridge.EnqueueCommand(new ConditionSetCommand(checkpointNumber, condition));
            await AwaitWithTimeoutAsync(command.Response, response =>
                AnsiConsole.MarkupLine($"Set response: {response.ErrorCode} and response type {response.Response?.GetType().Name}"));
        }
        async Task MemoryGetAsync(CancellationToken ct)
        {
            var command = bridge.EnqueueCommand(new MemoryGetCommand(0, 0x0812, 0x081A, MemSpace.MainMemory, 0));
            await AwaitWithTimeoutAsync(command.Response, response =>
            {
                if (response.Response?.Memory is not null)
                {
                    var buffer = response.Response.Memory.Value;
                    string data = string.Join(" ", buffer.Data.Take((int)buffer.Size).Select(b => $"${b:X2}"));
                    AnsiConsole.MarkupLine($"Set response: {response.ErrorCode}: [bold]{data}[/]");
                    response.Response.Memory.Value.Dispose();
                }
                else
                {
                    AnsiConsole.MarkupLine($"Set response: {response.ErrorCode} [red]without data[/red]");
                }
            });
        }
        byte counter = 0;
        async Task MemorySetAsync(CancellationToken ct)
        {
            using (var buffer = BufferManager.GetBuffer(8))
            {
                // makes sample content where first byte changes each time method is called to verify that VICE data has been indeed changed
                buffer.Data[0] = counter;
                for (int i = 1; i < 8; i++)
                {
                    buffer.Data[i] = (byte)i;
                }
                counter++;
                var command = bridge.EnqueueCommand(
                    new MemorySetCommand(0, 0x0812, MemSpace.MainMemory, 0, buffer));
                await AwaitWithTimeoutAsync(command.Response, response =>
                    AnsiConsole.MarkupLine($"Set response: {response.ErrorCode} and response type {response.Response?.GetType().Name}"));
            }
        }
        async Task ViceInfoAsync(CancellationToken ct)
        {
            var command = new InfoCommand();
            bridge.EnqueueCommand(command);
            Action<CommandResponse<InfoResponse>> onSuccess = cr =>
            {
                var response = cr.Response!;
                AnsiConsole.MarkupLine($"VICE version number is [bold]{response.Major}.{response.Minor}.{response.Build}.{response.Revision}[/], SVN version is [bold]{response.SvnVersion}[/]");
            };
            await AwaitWithTimeoutAsync(command.Response, onSuccess);
        }
        async Task GetDisplayAsync(CancellationToken ct)
        {
            var command = new DisplayGetCommand(UseVic: true, ImageFormat.Indexed);
            bridge.EnqueueCommand(command);
            Action<CommandResponse<DisplayGetResponse>> onSuccess = async commandResponse =>
            {

                var response = commandResponse.Response;
                try
                {
                    AnsiConsole.MarkupLine($"Got response [bold]{response!.GetType().Name}[/]");
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
                    response!.Dispose();
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

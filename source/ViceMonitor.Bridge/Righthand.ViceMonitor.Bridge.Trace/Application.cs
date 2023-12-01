using Microsoft.Extensions.Logging;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Services.Abstract;
using Spectre.Console;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Righthand.ViceMonitor.Bridge.Trace
{
    public class Application
    {
        readonly ILogger logger;
        readonly IViceBridge bridge;
        ImmutableDictionary<byte, FullRegisterItem> registers;
        uint? tracepointNumber;
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
                    try
                    {
                        await StartUpAsync(ct);
                        AnsiConsole.Clear();
                        bridge.PerformanceProfiler.Clear();
                        Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteException(ex);
                    }
                    finally
                    {
                        await CleanUp(ct);
                    }
                    ShowPerformanceData();
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
        Stopwatch? swResumed;
        Stopwatch? swStopped;
        Stopwatch? swInfo;
        long minStoppedTicks = 1_000_000, maxStoppedTicks;
        long minInfoTicks = 1_000_000, maxInfoTicks;
        int rounds;
        async void Bridge_ViceResponse(object? sender, ViceResponseEventArgs e)
        {
            //AnsiConsole.MarkupLine($"Got unbound [bold]{e.Response.GetType().Name}[/]");
            switch (e.Response)
            {
                case ResumedResponse:
                    swResumed = Stopwatch.StartNew();
                    //ShowInfo(0, "Stopped to resumed", ref minInfoTicks, ref maxInfoTicks, swStopped);
                    break;
                case RegistersResponse registers:
                    //OutputRegisters(registers.Items);
                    break;
                case StoppedResponse:
                    if (tracepointNumber.HasValue)
                    {
                        var exitCommand = bridge.EnqueueCommand(new ExitCommand());
                        await exitCommand.Response;
                    }
                    swStopped = Stopwatch.StartNew();
                    //ShowInfo(10, "Info to stopped", ref minStoppedTicks, ref maxStoppedTicks, swInfo);
                    break;
                case CheckpointInfoResponse:
                    swInfo = Stopwatch.StartNew();
                    //ShowInfo(2, "Resumed to info", swResumed);
                    break;
            }
        }
        void ShowPerformanceData()
        {
            string FormatTicks(long ticks) => ticks.ToString("#,##0").PadLeft(10);
            PerformanceEvent? previous = null;
            int index = 0;
            foreach (var e in bridge.PerformanceProfiler.Events.ToImmutableArray())
            {
                //string ticks = FormatTicks(e.Ticks);
                string indexText = index.ToString().PadLeft(6);
                long deltaTicks = previous is not null ? e.Ticks - previous.Ticks : 0;
                string delta = FormatTicks(deltaTicks);
                var name = e.GetType().Name.AsSpan()[0..^5];
                string defaultText = $"{indexText}{delta} [bold]{name}[/]";
                switch (e)
                {
                    case StartListeningEvent startListening:
                        AnsiConsole.MarkupLine(defaultText);
                        break;
                    case DataAvailableEvent dataAvailable:
                        AnsiConsole.MarkupLine($"{defaultText} {dataAvailable.DataType}");
                        break;
                    case CommandSentEvent commandSent:
                        AnsiConsole.MarkupLine($"{defaultText} {commandSent.CommandType.Name}");
                        break;
                    case CommandCompletedEvent commandCompleted:
                        AnsiConsole.MarkupLine($"{defaultText} {commandCompleted.CommandType.Name}");
                        break;
                    case ResponseReadEvent responseRead:
                        string offsetTab = responseRead.IsNested ? "    " : "";
                        AnsiConsole.MarkupLine($"{defaultText} {offsetTab}{responseRead.ResponseType.Name}");
                        break;
                    case RawSendEvent rawSend:
                        string delays = rawSend.Delays > 0 ? $"[red]{rawSend.Delays}[/]" : rawSend.Delays.ToString();
                        string passes = rawSend.Passes > 1 ? $"[red]{rawSend.Passes}[/]" : rawSend.Passes.ToString();
                        AnsiConsole.MarkupLine($"{defaultText}     Passes:{passes} Delays:{delays}");
                        break;
                    case TraceEvent trace:
                        AnsiConsole.MarkupLine($"{defaultText} {trace.Info}");
                        break;
                    default:
                        AnsiConsole.MarkupLine(defaultText);
                        break;
                }
                previous = e;
                index++;
            }
        }
        void ShowInfo(int line, string label, ref long minTicks, ref long maxTicks, Stopwatch? sw)
        {
            if (sw is not null)
            {
                AnsiConsole.Cursor.SetPosition(0, line);
                string elapsed = sw.ElapsedMilliseconds.ToString("#,##0").PadLeft(12);
                string min = minTicks.ToString("#,##0").PadLeft(12);
                string max = maxTicks.ToString("#,##0").PadLeft(12);
                rounds++;
                if (rounds > 1_0 && sw.ElapsedMilliseconds > maxTicks)
                {
                    maxTicks = sw.ElapsedMilliseconds;
                }
                if (sw.ElapsedMilliseconds < minTicks)
                {
                    minTicks = sw.ElapsedMilliseconds;
                }
                
                AnsiConsole.WriteLine($"{label.PadLeft(20)} {elapsed} {min} {max} ms".PadLeft(40));
            }
        }

        
        internal async Task<CommandResponse<TResponse>?> AwaitWithTimeoutAsync<TResponse>(Task<CommandResponse<TResponse>> task, Action<CommandResponse<TResponse>> textOnSuccess)
            where TResponse : ViceResponse
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
        async Task LoadAndStartAppAsync(CancellationToken ct)
        {
            var file = Path.Combine(Path.GetDirectoryName(typeof(Application).Assembly.Location)!, "Samples", "dbglogtest.prg");
            var command = bridge.EnqueueCommand(new AutoStartCommand(runAfterLoading: true, 0, file));
            var response = await command.Response;
            if (response.Response is null)
            {
                throw new Exception($"Failed to get checkpoint list: {response.ErrorCode}");
            }
        }
        internal async Task StartUpAsync(CancellationToken ct)
        {
            AnsiConsole.WriteLine("Starting up");
            await DeleteCheckpointsAsync(ct);
            await TracepointSetAsync(ct);
            await LoadAndStartAppAsync(ct);
        }
        async Task CleanUp(CancellationToken ct)
        {
            if (tracepointNumber.HasValue)
            {
                await CheckpointDeleteAsync(tracepointNumber.Value, ct);
            }
        }
        async Task DeleteCheckpointsAsync(CancellationToken ct)
        {
            var listCommand = new CheckpointListCommand();
            bridge.EnqueueCommand(listCommand);
            var listResult = await listCommand.Response.WaitAsync(ct);
            foreach (var c in listResult.Response?.Info ?? throw new Exception($"Failed to get checkpoit list: {listResult.ErrorCode}"))
            {
                AnsiConsole.WriteLine($"Deleting checkpoint [bold]{c.CheckpointNumber}[/]");
                await CheckpointDeleteAsync(c.CheckpointNumber, ct);
            }
            AnsiConsole.WriteLine("Checkpoints purged");
        }
        async Task TracepointSetAsync(CancellationToken ct)
        {
            var setCommand = bridge.EnqueueCommand(new CheckpointSetCommand(0xd4fc, 0xd4fc, StopWhenHit: true, Enabled: true,
               CpuOperation: CpuOperation.Store, Temporary: false));
            var result = await setCommand.Response.WaitAsync(ct);
            var response = result.Response ?? throw new Exception($"Failed to set tracepoint: {result.ErrorCode}");
            tracepointNumber = response.CheckpointNumber;
            AnsiConsole.MarkupLine($"Tracepoint set response: {response.ErrorCode} with Checkpoint Number {response.CheckpointNumber}");
        }
        async Task CheckpointDeleteAsync(uint checkpointNumber, CancellationToken ct)
        {
            var setCommand = bridge.EnqueueCommand(new CheckpointDeleteCommand(checkpointNumber));
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

        void Bridge_ConnectedChanged(object? sender, ConnectedChangedEventArgs e)
        {
            UpdateConnectedState();
        }
        void UpdateConnectedState()
        {
            AnsiConsole.MarkupLine($"Bridge is {(bridge.IsConnected ? "[green]connected[/]" : "[red]disconnected[/]")}");
        }
    }
}

using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Exit the monitor until the next breakpoint. 
    /// </summary>
    public record ExitCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.Exit)
    { }
}

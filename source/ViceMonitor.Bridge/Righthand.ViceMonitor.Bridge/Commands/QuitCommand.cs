using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Quits VICE. 
    /// </summary>
    public record QuitCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.Quit)
    { }
}

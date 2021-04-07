using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Get an empty response.
    /// </summary>
    public record PingCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.Ping)
    { }
}

using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Retrieves VICE version.
    /// </summary>
    public record InfoCommand(): ParameterlessCommand<InfoResponse>(CommandType.Info);
}

using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Gives a listing of all the bank IDs for the running machine with their names. 
    /// </summary>
    public record BanksAvailableCommand() : ParameterlessCommand<BanksAvailableResponse>(CommandType.BanksAvailable)
    { }
}

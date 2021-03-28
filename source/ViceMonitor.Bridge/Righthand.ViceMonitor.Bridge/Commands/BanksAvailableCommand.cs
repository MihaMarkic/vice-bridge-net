using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record BanksAvailableCommand() : ParameterlessCommand<BanksAvailableResponse>(CommandType.BanksAvailable)
    { }
}

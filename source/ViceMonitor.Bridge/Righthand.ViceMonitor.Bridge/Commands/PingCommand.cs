namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record PingCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.Ping)
    { }
}

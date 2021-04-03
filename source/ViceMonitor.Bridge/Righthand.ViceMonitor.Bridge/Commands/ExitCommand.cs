namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record ExitCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.Exit)
    { }
}

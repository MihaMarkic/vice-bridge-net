namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record QuitCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.Quit)
    { }
}

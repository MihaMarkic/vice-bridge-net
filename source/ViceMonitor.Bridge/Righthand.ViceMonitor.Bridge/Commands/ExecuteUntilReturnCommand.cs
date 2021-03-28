namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record ExecuteUntilReturnCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.ExecuteUntilReturn)
    { }
}

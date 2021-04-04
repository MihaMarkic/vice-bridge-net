namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Continues execution and returns to the monitor just after the next RTS or RTI is executed.
    /// </summary>
    /// <remarks>This command is the same as "return" in the text monitor.</remarks>
    public record ExecuteUntilReturnCommand() : ParameterlessCommand<EmptyViceResponse>(CommandType.ExecuteUntilReturn)
    { }
}

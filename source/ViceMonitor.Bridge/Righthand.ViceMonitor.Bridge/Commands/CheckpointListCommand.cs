namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Lists checkpoints.
    /// </summary>
    /// <remarks>
    /// Emits a series of MON_RESPONSE_CHECKPOINT_INFO responses (see section 13.5.1 Checkpoint Response (0x11)) followed by0x14: MON_RESPONSE_CHECKPOINT_LIST
    /// </remarks>
    public record CheckpointListCommand() : ParameterlessCommand<CheckpointResponse>(CommandType.CheckpointList)
    {
    }
}

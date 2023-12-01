using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Deletes any type of checkpoint. (break, watch, trace) 
    /// </summary>
    /// <param name="CheckpointNumber"></param>
    public record CheckpointDeleteCommand(uint CheckpointNumber) : ViceCommand<EmptyViceResponse>(CommandType.CheckpointDelete)
    {
        /// <inheritdoc />
        public override uint ContentLength => sizeof(uint);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, CheckpointNumber);
        }
    }
}

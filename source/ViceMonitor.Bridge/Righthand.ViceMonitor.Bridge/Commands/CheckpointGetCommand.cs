using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Gets any type of checkpoint. (break, watch, trace)
    /// </summary>
    /// <param name="CheckpointNumber"></param>
    public record CheckpointGetCommand(uint CheckpointNumber) : ViceCommand<CheckpointInfoResponse>(CommandType.CheckpointGet)
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

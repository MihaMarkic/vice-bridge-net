using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Deletes any type of checkpoint. (break, watch, trace) 
    /// </summary>
    /// <param name="CheckpointNumber"></param>
    public record CheckpointDeleteCommand(uint CheckpointNumber) : ViceCommand<CheckpointInfoResponse>(CommandType.CheckpointDelete)
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

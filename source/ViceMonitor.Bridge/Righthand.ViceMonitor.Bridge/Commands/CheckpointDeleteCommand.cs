using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record CheckpointDeleteCommand(uint CheckpointNumber) : ViceCommand<CheckpointResponse>(CommandType.CheckpointDelete)
    {
        public override uint ContentLength => sizeof(uint);
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, CheckpointNumber);
        }
    }
}

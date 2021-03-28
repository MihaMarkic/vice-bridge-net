using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record CheckpointGetCommand(uint CheckpointNumber) : ViceCommand<CheckpointResponse>(CommandType.CheckpointGet)
    {
        public override uint ContentLength => sizeof(uint);
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, CheckpointNumber);
        }
    }
}

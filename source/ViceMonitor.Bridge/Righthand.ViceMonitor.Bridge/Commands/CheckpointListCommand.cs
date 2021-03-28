using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record CheckpointListCommand(uint TotalCheckpointsNumber) : ViceCommand<CheckpointResponse>(CommandType.CheckpointList)
    {
        public override uint ContentLength => sizeof(uint);
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, TotalCheckpointsNumber);
        }
    }

    public record CheckpointToggleCommand(uint CheckpointNumber, bool Enabled) : ViceCommand<CheckpointResponse>(CommandType.CheckpointToggle)
    {
        public override uint ContentLength => sizeof(uint) + sizeof(byte);
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, CheckpointNumber);
            buffer[4] = Enabled.AsByte();
        }
    }
}

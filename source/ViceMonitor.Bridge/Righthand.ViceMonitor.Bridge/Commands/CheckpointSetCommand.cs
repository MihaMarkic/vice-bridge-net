using System;
using System.Text;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record CheckpointSetCommand(ushort StartAddress, ushort EndAddress, bool StopWhenHit, bool Enabled, CpuOperation CpuOperation,
        bool Temporary)
        : ViceCommand<CheckpointResponse>(CommandType.CheckpointSet)
    {
        public override uint ContentLength { get; } = sizeof(ushort) + sizeof(ushort) + sizeof(bool) + sizeof(bool) + sizeof(CpuOperation) + sizeof(bool);
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer[0..], StartAddress);
            BitConverter.TryWriteBytes(buffer[2..], EndAddress);
            buffer[4] = StopWhenHit.AsByte();
            buffer[5] = Enabled.AsByte();
            buffer[6] = (byte)CpuOperation;
            buffer[7] = Temporary.AsByte();
        }
    }
}

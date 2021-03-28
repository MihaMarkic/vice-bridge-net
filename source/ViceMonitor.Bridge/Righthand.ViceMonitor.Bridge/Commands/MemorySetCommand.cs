using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record MemorySetCommand(byte SideEffects, ushort StartAddress, ushort EndAddress, MemSpace MemSpace, ushort BankId, ManagedBuffer MemoryContent)
        : ViceCommand<EmptyViceResponse>(CommandType.MemorySet)
    {
        public override uint ContentLength { get; } = sizeof(byte) + sizeof(ushort) + sizeof(ushort) + sizeof(byte) + sizeof(ushort) + MemoryContent.Size;
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = SideEffects;
            BitConverter.TryWriteBytes(buffer[1..], StartAddress);
            BitConverter.TryWriteBytes(buffer[3..], EndAddress);
            buffer[5] = (byte)MemSpace;
            BitConverter.TryWriteBytes(buffer[6..], BankId);
            MemoryContent.Data.CopyTo(buffer[8..]);
        }
    }
}

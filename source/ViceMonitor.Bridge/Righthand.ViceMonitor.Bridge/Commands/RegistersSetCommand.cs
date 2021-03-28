using System;
using System.Collections.Immutable;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record RegistersSetCommand(MemSpace MemSpace, ImmutableArray<RegisterItem> Items) : ViceCommand<RegistersResponse>(CommandType.RegistersSet)
    {
        public RegistersSetCommand(MemSpace MemSpace, params RegisterItem[] args) : this(MemSpace, args.ToImmutableArray())
        { }
        public override uint ContentLength => sizeof(MemSpace) + sizeof(ushort) + (uint)Items.Length * RegisterItem.ContentLength;
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)MemSpace;
            BitConverter.TryWriteBytes(buffer[1..], (ushort)Items.Length);
            for (int i = 0; i < Items.Length; i++)
            {
                var itemBuffer = buffer.Slice(2 + i * (int)RegisterItem.ContentLength);
                var item = Items[i];
                itemBuffer[0] = RegisterItem.Size;
                itemBuffer[1] = item.RegisterId;
                BitConverter.TryWriteBytes(itemBuffer[2..], item.RegisterValue);
            }
        }
    }
}

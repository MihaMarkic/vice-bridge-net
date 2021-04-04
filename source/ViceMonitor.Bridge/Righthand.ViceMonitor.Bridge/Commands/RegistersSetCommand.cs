using System;
using System.Collections.Immutable;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Set the register values
    /// </summary>
    /// <param name="MemSpace">Describes which part of the computer you want to write</param>
    /// <param name="Items">
    /// An array with items of structure:
    ///   byte 0: Size of the item, excluding this byte 1: ID of the register byte 2-3: register value
    /// </param>
    public record RegistersSetCommand(MemSpace MemSpace, ImmutableArray<RegisterItem> Items) : ViceCommand<RegistersResponse>(CommandType.RegistersSet)
    {
        /// <summary>
        /// Initializes an instance of <see cref="RegistersSetCommand"/>.
        /// </summary>
        /// <param name="MemSpace">Describes which part of the computer you want to write</param>
        /// <param name="args">
        /// An array with items of structure:
        ///   byte 0: Size of the item, excluding this byte 1: ID of the register byte 2-3: register value
        /// </param>
        public RegistersSetCommand(MemSpace MemSpace, params RegisterItem[] args) : this(MemSpace, args.ToImmutableArray())
        { }
        /// <inheritdoc />
        public override uint ContentLength => sizeof(MemSpace) + sizeof(ushort) + (uint)Items.Length * RegisterItem.ContentLength;
        /// <inheritdoc />
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

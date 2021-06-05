using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Writes a chunk of memory from a start address to an end address (inclusive). 
    /// </summary>
    /// <param name="SideEffects">Should the write cause side effects? </param>
    /// <param name="StartAddress"></param>
    /// <param name="EndAddress"></param>
    /// <param name="MemSpace">Describes which part of the computer you want to write.</param>
    /// <param name="BankId">Describes which bank you want. This is dependent on your machine. If the memspace selected doesn't support banks, this value is ignored. </param>
    /// <param name="MemoryContent">Memory content to set.</param>
    /// <remarks>MemoryContent will be disposed together with this command.</remarks>
    public record MemorySetCommand(byte SideEffects, ushort StartAddress, ushort EndAddress, MemSpace MemSpace, ushort BankId, ManagedBuffer MemoryContent)
        : ViceCommand<EmptyViceResponse>(CommandType.MemorySet), IDisposable
    {
        /// <inheritdoc />
        public override uint ContentLength { get; } = sizeof(byte) + sizeof(ushort) + sizeof(ushort) + sizeof(byte) + sizeof(ushort) + MemoryContent.Size;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = SideEffects;
            BitConverter.TryWriteBytes(buffer[1..], StartAddress);
            BitConverter.TryWriteBytes(buffer[3..], EndAddress);
            buffer[5] = (byte)MemSpace;
            BitConverter.TryWriteBytes(buffer[6..], BankId);
            MemoryContent.Data[0..(int)MemoryContent.Size].CopyTo(buffer[8..]);
        }
        /// <summary>
        /// Releases all resources used by the <see cref="MemorySetCommand"/>.
        /// </summary>
        public void Dispose()
        {
            MemoryContent.Dispose();
        }
    }
}

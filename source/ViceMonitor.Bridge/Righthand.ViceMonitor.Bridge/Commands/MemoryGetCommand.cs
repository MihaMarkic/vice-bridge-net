using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Reads a chunk of memory from a start address to an end address (inclusive). 
    /// </summary>
    /// <param name="SideEffects">Should the read cause side effects?</param>
    /// <param name="StartAddress"></param>
    /// <param name="EndAddress"></param>
    /// <param name="MemSpace">Describes which part of the computer you want to read.</param>
    /// <param name="BankId">Describes which bank you want. This is dependent on your machine. If the memspace selected doesn't support banks, this value is ignored. </param>
    public record MemoryGetCommand(byte SideEffects, ushort StartAddress, ushort EndAddress, MemSpace MemSpace, ushort BankId)
        : ViceCommand<EmptyViceResponse>(CommandType.MemoryGet)
    {
        /// <inheritdoc />
        public override uint ContentLength { get; } = sizeof(byte) + sizeof(ushort) + sizeof(ushort) + sizeof(byte) + sizeof(ushort);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = SideEffects;
            BitConverter.TryWriteBytes(buffer[1..], StartAddress);
            BitConverter.TryWriteBytes(buffer[3..], EndAddress);
            buffer[5] = (byte)MemSpace;
            BitConverter.TryWriteBytes(buffer[6..], BankId);
        }
    }
}

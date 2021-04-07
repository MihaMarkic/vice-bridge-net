using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Gives a listing of all the registers for the running machine with their names. 
    /// </summary>
    /// <param name="MemSpace">Describes which part of the computer you want to read.</param>
    public record RegistersAvailableCommand(MemSpace MemSpace) : ViceCommand<RegistersAvailableResponse>(CommandType.RegistersAvailable)
    {
        /// <inheritdoc />
        public override uint ContentLength => sizeof(MemSpace);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)MemSpace;
        }
    }
}

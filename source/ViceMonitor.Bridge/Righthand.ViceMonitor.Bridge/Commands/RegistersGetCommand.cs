using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Get details about the registers 
    /// </summary>
    /// <param name="MemSpace">Describes which part of the computer you want to read.</param>
    public record RegistersGetCommand(MemSpace MemSpace) : ViceCommand<RegistersResponse>(CommandType.RegistersGet)
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

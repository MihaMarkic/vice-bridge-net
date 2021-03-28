using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record RegistersAvailableCommand(MemSpace MemSpace) : ViceCommand<RegistersAvailableResponse>(CommandType.RegistersAvailable)
    {
        public override uint ContentLength => sizeof(MemSpace);
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)MemSpace;
        }
    }
}

using System;
using System.Collections.Immutable;
using System.Data;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record RegistersGetCommand(MemSpace MemSpace) : ViceCommand<RegistersResponse>(CommandType.RegistersGet)
    {
        public override uint ContentLength => sizeof(MemSpace);
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)MemSpace;
        }
    }
}

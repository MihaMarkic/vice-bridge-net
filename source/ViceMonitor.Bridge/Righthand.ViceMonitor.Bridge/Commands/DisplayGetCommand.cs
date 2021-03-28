using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record DisplayGetCommand(bool UseVic, ImageFormat Format) : ViceCommand<DisplayGetResponse>(CommandType.DisplayGet)
    {
        public override uint ContentLength => sizeof(byte) + sizeof(byte);
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = UseVic.AsByte();
            buffer[1] = (byte)Format;
        }
    }
}

using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record ResetCommand(ResetMode Mode) : ParameterlessCommand<EmptyViceResponse>(CommandType.Reset)
    {
        public override uint ContentLength => sizeof(ResetMode);
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)Mode;
            base.WriteContent(buffer);
        }
    }

    public enum ResetMode: byte
    {
        Soft    = 0x00,
        Hard    = 0x01,
        Drive8  = 0x08,
        Drive9  = 0x09,
        Drive10 = 0x0a,
        Drive11 = 0x0b,
    }
}

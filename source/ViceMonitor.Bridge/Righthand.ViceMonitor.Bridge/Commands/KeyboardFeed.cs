using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record KeyboardFeed : ViceCommand<EmptyViceResponse>
    {
        public string Text { get; init; }
        public override uint ContentLength => sizeof(byte) + (uint)Text.Length;
        public KeyboardFeed(string text) : base(CommandType.KeyboardFeed)
        {
            // TODO escape text
            Text = text;
            if (text.Length > 256)
            {
                throw new ArgumentException($"Maximum escaped text length is 256 chars: '{Text}'", nameof(text));
            }
        }
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)Text.Length;
            WriteString(Text, buffer[1..]);
        }
    }
}

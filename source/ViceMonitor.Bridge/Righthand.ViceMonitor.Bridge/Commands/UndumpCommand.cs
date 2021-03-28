using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record UndumpCommand : ViceCommand<UndumpResponse>
    {
        public string Filename { get; init; }
        public UndumpCommand(string filename) : base(CommandType.Undump)
        {
            if (filename.Length > 256)
            {
                throw new ArgumentException($"Maximum filename length is 256 chars", nameof(filename));
            }
            Filename = filename;
        }
        public override uint ContentLength => sizeof(ushort) + (uint)Filename.Length;
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)Filename.Length;
            WriteString(Filename, buffer[1..]);
        }
    }
}

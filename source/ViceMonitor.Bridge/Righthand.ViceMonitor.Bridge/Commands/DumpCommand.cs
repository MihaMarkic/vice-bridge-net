using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record DumpCommand : ViceCommand<EmptyViceResponse>
    {
        public bool SaveRom { get; init; }
        public bool SaveDisks { get; init; }
        public string Filename { get; init; }
        public DumpCommand(bool saveRom, bool saveDisks, string filename) : base(CommandType.Dump)
        {
            if (filename.Length > 256)
            {
                throw new ArgumentException($"Maximum filename length is 256 chars", nameof(filename));
            }
            SaveRom = saveRom;
            SaveDisks = saveDisks;
            Filename = filename;
        }
        public override uint ContentLength => sizeof(byte) + sizeof(byte) + sizeof(byte) + (uint)Filename.Length;
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = SaveRom.AsByte();
            buffer[1] = SaveDisks.AsByte();
            buffer[2] = (byte)Filename.Length;
            WriteString(Filename, buffer[3..]);
        }
    }
}

using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record AutoStartCommand : ViceCommand<EmptyViceResponse>
    {
        public bool RunAfterLoading { get; init; }
        public ushort FileIndex { get; init; }
        public string Filename { get; init; }
        public AutoStartCommand(bool runAfterLoading, ushort fileIndex, string filename) : base(CommandType.Quit)
        {
            RunAfterLoading = runAfterLoading;
            FileIndex = fileIndex;
            Filename = filename;
        }
        public override uint ContentLength => sizeof(byte) + sizeof(ushort) + sizeof(byte) + (uint)Filename.Length;
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = RunAfterLoading.AsByte();
            BitConverter.TryWriteBytes(buffer[1..], FileIndex);
            buffer[3] = (byte)Filename.Length;
            WriteString(Filename, buffer[4..]);
        }
    }
}

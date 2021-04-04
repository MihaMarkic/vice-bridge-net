using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Load a program then return to the monitor 
    /// </summary>
    public record AutoStartCommand : ViceCommand<EmptyViceResponse>
    {
        /// <summary>
        /// Run after loading?
        /// </summary>
        public bool RunAfterLoading { get; init; }
        /// <summary>
        /// The index of the file to execute, if a disk image. 0x00 is the default value.
        /// </summary>
        public ushort FileIndex { get; init; }
        /// <summary>
        /// The filename to autoload. 
        /// </summary>
        public string Filename { get; init; }
        /// <summary>
        /// Creates an instance of <see cref="AutoStartCommand"/>.
        /// </summary>
        /// <param name="runAfterLoading">Run after loading?</param>
        /// <param name="fileIndex">The index of the file to execute, if a disk image. 0x00 is the default value.</param>
        /// <param name="filename">The filename to autoload. </param>
        public AutoStartCommand(bool runAfterLoading, ushort fileIndex, string filename) : base(CommandType.Quit)
        {
            RunAfterLoading = runAfterLoading;
            FileIndex = fileIndex;
            Filename = filename;
        }
        /// <inheritdoc />
        public override uint ContentLength => sizeof(byte) + sizeof(ushort) + sizeof(byte) + (uint)Filename.Length;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = RunAfterLoading.AsByte();
            BitConverter.TryWriteBytes(buffer[1..], FileIndex);
            buffer[3] = (byte)Filename.Length;
            WriteString(Filename, buffer[4..]);
        }
    }
}

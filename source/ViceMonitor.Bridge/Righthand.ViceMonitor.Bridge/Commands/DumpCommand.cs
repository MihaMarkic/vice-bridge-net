using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Saves the machine state to a file. 
    /// </summary>
    public record DumpCommand : ViceCommand<EmptyViceResponse>
    {
        /// <summary>
        /// Save ROMs to snapshot file?
        /// </summary>
        public bool SaveRom { get; init; }
        /// <summary>
        /// Save disks to snapshot file?
        /// </summary>
        public bool SaveDisks { get; init; }
        /// <summary>
        /// The filename to save the snapshot to.
        /// </summary>
        public string Filename { get; init; }
        /// <summary>
        /// Creates an instance of <see cref="DumpCommand"/>.
        /// </summary>
        /// <param name="saveRom">Save ROMs to snapshot file?</param>
        /// <param name="saveDisks">Save disks to snapshot file?</param>
        /// <param name="filename">The filename to save the snapshot to.</param>
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
        /// <inheritdoc />
        public override uint ContentLength => sizeof(byte) + sizeof(byte) + sizeof(byte) + (uint)Filename.Length;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = SaveRom.AsByte();
            buffer[1] = SaveDisks.AsByte();
            buffer[2] = (byte)Filename.Length;
            WriteString(Filename, buffer[3..]);
        }
    }
}

using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Loads the machine state from a file.
    /// </summary>
    public record UndumpCommand : ViceCommand<UndumpResponse>
    {
        /// <summary>
        /// The filename to load the snapshot from. 
        /// </summary>
        public string Filename { get; init; }
        /// <summary>
        /// Creates an instance of <see cref="UndumpCommand"/>.
        /// </summary>
        /// <param name="filename">The filename to load the snapshot from. </param>
        public UndumpCommand(string filename) : base(CommandType.Undump)
        {
            if (filename.Length > 256)
            {
                throw new ArgumentException($"Maximum filename length is 256 chars", nameof(filename));
            }
            Filename = filename;
        }
        /// <inheritdoc />
        public override uint ContentLength => sizeof(ushort) + (uint)Filename.Length;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)Filename.Length;
            WriteString(Filename, buffer[1..]);
        }
    }
}

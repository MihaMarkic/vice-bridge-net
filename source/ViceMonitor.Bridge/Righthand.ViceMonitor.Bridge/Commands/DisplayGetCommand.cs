using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Gets the current screen in a requested bit format. 
    /// </summary>
    /// <param name="UseVic">Must be included, but ignored for all but the C128. If true, (0x01) the screen returned will be from the VIC-II. If false (0x00), it will be from the VDC.</param>
    /// <param name="Format"></param>
    public record DisplayGetCommand(bool UseVic, ImageFormat Format) : ViceCommand<DisplayGetResponse>(CommandType.DisplayGet)
    {
        /// <inheritdoc />
        public override uint ContentLength => sizeof(byte) + sizeof(byte);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = UseVic.AsByte();
            buffer[1] = (byte)Format;
        }
    }
}

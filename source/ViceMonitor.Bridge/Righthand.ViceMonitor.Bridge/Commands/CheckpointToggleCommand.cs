using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Checkpoint toggle.
    /// </summary>
    /// <param name="CheckpointNumber"></param>
    /// <param name="Enabled"></param>
    public record CheckpointToggleCommand(uint CheckpointNumber, bool Enabled) : ViceCommand<EmptyViceResponse>(CommandType.CheckpointToggle)
    {
        /// <inheritdoc />
        public override uint ContentLength => sizeof(uint) + sizeof(byte);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, CheckpointNumber);
            buffer[4] = Enabled.AsByte();
        }
    }
}

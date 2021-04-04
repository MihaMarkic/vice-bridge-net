using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Reset the system or a drive 
    /// </summary>
    /// <param name="Mode">What to reset</param>
    public record ResetCommand(ResetMode Mode) : ParameterlessCommand<EmptyViceResponse>(CommandType.Reset)
    {
        /// <inheritdoc />
        public override uint ContentLength => sizeof(ResetMode);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)Mode;
            base.WriteContent(buffer);
        }
    }

    /// <summary>
    /// Reset target
    /// </summary>
    public enum ResetMode: byte
    {
        /// <summary>
        /// Soft reset system 
        /// </summary>
        Soft = 0x00,
        /// <summary>
        /// Hard reset system 
        /// </summary>
        Hard = 0x01,
        /// <summary>
        /// Reset drive 8
        /// </summary>
        Drive8 = 0x08,
        /// <summary>
        /// Reset drive 9
        /// </summary>
        Drive9 = 0x09,
        /// <summary>
        /// Reset drive 10
        /// </summary>
        Drive10 = 0x0a,
        /// <summary>
        /// Reset drive 11
        /// </summary>
        Drive11 = 0x0b,
    }
}

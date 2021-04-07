using Righthand.ViceMonitor.Bridge.Responses;
using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Sets any type of checkpoint. This combines the functionality of several textual commands (break, watch, trace) into one, as they are all the same with only minor variations. To set conditions, see section 13.4.8 Condition set (0x22) after executing this one.
    /// </summary>
    /// <param name="StartAddress"></param>
    /// <param name="EndAddress"></param>
    /// <param name="StopWhenHit"></param>
    /// <param name="Enabled"></param>
    /// <param name="CpuOperation"></param>
    /// <param name="Temporary">Deletes the checkpoint after it has been hit once. This is similar to "until" command, but it will not resume the emulator. </param>
    public record CheckpointSetCommand(ushort StartAddress, ushort EndAddress, bool StopWhenHit, bool Enabled, CpuOperation CpuOperation,
        bool Temporary)
        : ViceCommand<CheckpointInfoResponse>(CommandType.CheckpointSet)
    {
        /// <inheritdoc />
        public override uint ContentLength { get; } = sizeof(ushort) + sizeof(ushort) + sizeof(bool) + sizeof(bool) + sizeof(CpuOperation) + sizeof(bool);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer[0..], StartAddress);
            BitConverter.TryWriteBytes(buffer[2..], EndAddress);
            buffer[4] = StopWhenHit.AsByte();
            buffer[5] = Enabled.AsByte();
            buffer[6] = (byte)CpuOperation;
            buffer[7] = Temporary.AsByte();
        }
    }
}

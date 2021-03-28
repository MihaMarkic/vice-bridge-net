using System;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record AdvanceInstructionCommand(bool StepOverSubroutine, ushort NumberOfInstructions) : ViceCommand<EmptyViceResponse>(CommandType.AdvanceInstruction)
    {
        public override uint ContentLength => sizeof(byte) + sizeof(ushort);
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = StepOverSubroutine.AsByte();
            BitConverter.TryWriteBytes(buffer[1..], NumberOfInstructions);
        }
    }
}

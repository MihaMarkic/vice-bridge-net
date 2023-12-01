using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Step over a certain number of instructions. 
    /// </summary>
    /// <param name="StepOverSubroutine">Should subroutines count as a single instruction?</param>
    /// <param name="NumberOfInstructions">How many instructions to step over.</param>
    public record AdvanceInstructionCommand(bool StepOverSubroutine, ushort NumberOfInstructions) : ViceCommand<EmptyViceResponse>(CommandType.AdvanceInstruction)
    {
        /// <inheritdoc />
        public override uint ContentLength => sizeof(byte) + sizeof(ushort);
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = StepOverSubroutine.AsByte();
            BitConverter.TryWriteBytes(buffer[1..], NumberOfInstructions);
        }
    }
}

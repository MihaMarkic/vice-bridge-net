using System;
using System.Text;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record ConditionSetCommand : ViceCommand<EmptyViceResponse>
    {
        public uint CheckpointNumber { get; init; }
        public string ConditionExpression { get; init; }
        public ConditionSetCommand(uint checkpointNumber, string conditionExpression) : base(CommandType.ConditionSet)
        {
            if (conditionExpression.Length > 256)
            {
                throw new ArgumentException($"Maximum condition expression length is 256 chars", nameof(conditionExpression));
            }
            CheckpointNumber = checkpointNumber;
            ConditionExpression = conditionExpression;
        }
        public override uint ContentLength => sizeof(uint) + (uint)ConditionExpression.Length;
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, CheckpointNumber);
            buffer[4] = (byte)ConditionExpression.Length;
            var encoder = Encoding.ASCII.GetEncoder();
            WriteString(ConditionExpression, buffer[5..]);
        }
    }
}

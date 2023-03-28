using Righthand.ViceMonitor.Bridge.Responses;
using System;
using System.Text;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Sets a condition on an existing checkpoint. It is not currently possible to retrieve conditions after setting them. 
    /// </summary>
    public record ConditionSetCommand : ViceCommand<EmptyViceResponse>
    {
        /// <summary></summary>
        public uint CheckpointNumber { get; init; }
        /// <summary>
        /// This is the same format used on the command line.
        /// </summary>
        public string ConditionExpression { get; init; }
        /// <summary>
        /// Initializes an instance of <see cref="ConditionSetCommand"/>.
        /// </summary>
        /// <param name="checkpointNumber"></param>
        /// <param name="conditionExpression">This is the same format used on the command line.</param>
        public ConditionSetCommand(uint checkpointNumber, string conditionExpression) : base(CommandType.ConditionSet)
        {
            if (conditionExpression.Length > 256)
            {
                throw new ArgumentException($"Maximum condition expression length is 256 chars", nameof(conditionExpression));
            }
            CheckpointNumber = checkpointNumber;
            ConditionExpression = conditionExpression;
        }
        /// <inheritdoc />
        public override uint ContentLength => sizeof(uint) + sizeof(byte) + (uint)ConditionExpression.Length;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            BitConverter.TryWriteBytes(buffer, CheckpointNumber);
            buffer[4] = (byte)ConditionExpression.Length;
            var encoder = Encoding.ASCII.GetEncoder();
            WriteString(ConditionExpression, buffer[5..]);
        }
    }
}

using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Add text to the keyboard buffer. 
    /// </summary>
    public record KeyboardFeedCommand : ViceCommand<EmptyViceResponse>
    {
        /// <summary>
        /// Special characters such as return are escaped with backslashes. 
        /// </summary>
        public string Text { get; init; }
        /// <summary>
        /// Creates an instance of <see cref="KeyboardFeedCommand"/>.
        /// </summary>
        /// <param name="text"></param>
        public KeyboardFeedCommand(string text) : base(CommandType.KeyboardFeed)
        {
            // TODO escape text
            Text = text;
            if (text.Length > 256)
            {
                throw new ArgumentException($"Maximum escaped text length is 256 chars: '{Text}'", nameof(text));
            }
        }
        /// <inheritdoc />
        public override uint ContentLength => sizeof(byte) + (uint)Text.Length;
        /// <inheritdoc />
        public override void WriteContent(Span<byte> buffer)
        {
            buffer[0] = (byte)Text.Length;
            WriteString(Text, buffer[1..]);
        }
    }
}

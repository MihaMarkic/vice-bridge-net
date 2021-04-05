namespace Righthand.ViceMonitor.Bridge.Shared
{
    /// <summary>
    /// Base resource type.
    /// </summary>
    public abstract record Resource
    {
        /// <summary>
        /// Length of the resource.
        /// </summary>
        public abstract byte Length { get; }
    }
    /// <summary>
    /// String resource.
    /// </summary>
    public record StringResource(string Text) : Resource
    {
        /// <inheritdoc />
        public override byte Length => (byte)(sizeof(byte) + sizeof(byte) + Text.Length);
    }
    /// <summary>
    /// Integer resource.
    /// </summary>
    public record IntegerResource(int Value) : Resource
    {
        /// <inheritdoc />
        public override byte Length => sizeof(byte) + sizeof(byte) + sizeof(int);
    }
}

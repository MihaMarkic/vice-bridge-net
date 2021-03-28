namespace Righthand.ViceMonitor.Bridge.Commands
{
    public abstract record Resource
    {
        public abstract byte Length { get; }
    }
    public record StringResource(string Text) : Resource
    {
        public override byte Length => (byte)(sizeof(byte) + sizeof(byte) + Text.Length);
    }
    public record IntegerResource(int Value) : Resource
    {
        public override byte Length => (byte)(sizeof(byte) + sizeof(byte) + sizeof(int));
    }
}

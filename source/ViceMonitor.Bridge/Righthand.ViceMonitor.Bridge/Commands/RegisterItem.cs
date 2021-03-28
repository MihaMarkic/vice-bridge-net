namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record RegisterItem(byte RegisterId, ushort RegisterValue)
    {
        public const byte Size = 3;
        public const uint ContentLength = 4;
    }
}

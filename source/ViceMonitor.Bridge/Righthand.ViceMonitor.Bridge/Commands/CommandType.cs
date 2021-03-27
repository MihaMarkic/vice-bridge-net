namespace Righthand.ViceMonitor.Bridge.Commands
{
    public enum CommandType: byte
    {
        Temp =0xFF,
        CheckpointSet = 0x12,
        MemoryGet = 0x01,
    }
}

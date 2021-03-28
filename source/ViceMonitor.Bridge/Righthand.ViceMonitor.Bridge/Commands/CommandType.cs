namespace Righthand.ViceMonitor.Bridge.Commands
{
    public enum CommandType: byte
    {
        MemoryGet           = 0x01,
        MemorySet           = 0x02,
        CheckpointGet       = 0x11,
        CheckpointSet       = 0x12,
        CheckpointDelete    = 0x13,
        CheckpointList      = 0x14,
        CheckpointToggle    = 0x15,
        ConditionSet        = 0x22,
        RegistersGet        = 0x31,
        RegistersSet        = 0x32,
        Dump                = 0x41,
        Undump              = 0x42,
        ResourceGet         = 0x51,
        ResourceSet         = 0x52,
        AdvanceInstruction  = 0x71,
        KeyboardFeed        = 0x72,
        ExecuteUntilReturn  = 0x73,
        Ping                = 0x81,
        BanksAvailable      = 0x82,
        RegistersAvailable  = 0x83,
        DisplayGet          = 0x84,
    }
}

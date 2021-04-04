namespace Righthand.ViceMonitor.Bridge.Commands
{
    public enum ResponseType: byte
    {
        MemoryGet                   = 0x01,
        MemorySet                   = 0x02,
        /// <summary>
        /// MON_RESPONSE_CHECKPOINT_INFO 
        /// </summary>
        CheckpointInfo = 0x11,
        /// <summary>
        /// MON_RESPONSE_CHECKPOINT_LIST
        /// </summary>
        CheckpointList   = 0x14,
        Registers                   = 0x31,
        Dump                        = 0x41,
        Undump                      = 0x42,
        ResourceGet                 = 0x51,
        ResourceSet                 = 0x52,
        Jam                         = 0x61,
        Stopped                     = 0x62,
        Resumed                     = 0x63,
        AdvanceInstruction          = 0x71,
        KeyboardFeed                = 0x72,
        ExecuteUntilReturn          = 0x73,
        Ping                        = 0x81,
        BanksAvailable              = 0x82,
        RegistersAvailable          = 0x83,
        DisplayGet                  = 0x84,
        Exit                        = 0x88,
        Quit                        = 0xbb,
        Reset                       = 0xcc,
        AutoStart                   = 0xdd,
    }
}

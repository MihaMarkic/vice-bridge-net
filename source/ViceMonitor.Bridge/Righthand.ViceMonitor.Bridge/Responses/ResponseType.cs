namespace Righthand.ViceMonitor.Bridge.Responses
{
    /// <summary>
    /// VICE's response type codes
    /// </summary>
    public enum ResponseType: byte
    {
        /// <summary>
        /// MON_RESPONSE_MEM_GET
        /// </summary>
        MemoryGet                           = 0x01,
        /// <summary>
        /// MON_RESPONSE_MEM_SET
        /// </summary>
        MemorySet                           = 0x02,
        /// <summary>
        /// MON_RESPONSE_CHECKPOINT_INFO 
        /// </summary>
        CheckpointInfo                      = 0x11,
        /// <summary>
        /// MON_RESPONSE_CHECKPOINT_LIST
        /// </summary>
        CheckpointList                      = 0x14,
        /// <summary>
        /// MON_RESPONSE_CHECKPOINT_TOGGLE
        /// </summary>
        CheckpointToggle                    = 0x15,
        /// <summary>
        /// MON_RESPONSE_CONDITION_SET
        /// </summary>
        ConditionSet                        = 0x22,
        /// <summary>
        /// MON_RESPONSE_REGISTER_INFO
        /// </summary>
        RegisterInfo                        = 0x31,
        /// <summary>
        /// MON_RESPONSE_DUMP
        /// </summary>
        Dump                                = 0x41,
        /// <summary>
        /// MON_RESPONSE_UNDUMP
        /// </summary>
        Undump                              = 0x42,
        /// <summary>
        /// MON_RESPONSE_RESOURCE_GET
        /// </summary>
        ResourceGet                         = 0x51,
        /// <summary>
        /// MON_RESPONSE_RESOURCE_SET
        /// </summary>
        ResourceSet                         = 0x52,
        /// <summary>
        /// MON_RESPONSE_JAM
        /// </summary>
        Jam                                 = 0x61,
        /// <summary>
        /// MON_RESPONSE_STOPPED
        /// </summary>
        Stopped                             = 0x62,
        /// <summary>
        /// MON_RESPONSE_RESUMED
        /// </summary>
        Resumed                             = 0x63,
        /// <summary>
        /// MON_RESPONSE_ADVANCE_INSTRUCTIONS
        /// </summary>
        AdvanceInstruction                  = 0x71,
        /// <summary>
        /// MON_RESPONSE_KEYBOARD_FEED
        /// </summary>
        KeyboardFeed                        = 0x72,
        /// <summary>
        /// MON_RESPONSE_EXECUTE_UNTIL_RETURN
        /// </summary>
        ExecuteUntilReturn                  = 0x73,
        /// <summary>
        /// MON_RESPONSE_PING
        /// </summary>
        Ping                                = 0x81,
        /// <summary>
        /// MON_RESPONSE_BANKS_AVAILABLE
        /// </summary>
        BanksAvailable                      = 0x82,
        /// <summary>
        /// MON_RESPONSE_REGISTERS_AVAILABLE
        /// </summary>
        RegistersAvailable                  = 0x83,
        /// <summary>
        /// MON_RESPONSE_DISPLAY_GET
        /// </summary>
        DisplayGet                          = 0x84,
        /// <summary>
        /// MON_RESPONSE_INFO
        /// </summary>
        Info                                = 0x85,
        /// <summary>
        /// MON_RESPONSE_EXIT
        /// </summary>
        Exit                                = 0x88,
        /// <summary>
        /// MON_RESPONSE_QUIT
        /// </summary>
        Quit                                = 0xbb,
        /// <summary>
        /// MON_RESPONSE_RESET
        /// </summary>
        Reset                               = 0xcc,
        /// <summary>
        /// MON_RESPONSE_AUTOSTART
        /// </summary>
        AutoStart                           = 0xdd,
    }
}

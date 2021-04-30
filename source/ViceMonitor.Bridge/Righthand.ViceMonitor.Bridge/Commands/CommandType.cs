namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// VICE's command type codes
    /// </summary>
    public enum CommandType: byte
    {
        /// <summary>
        /// e_MON_CMD_INVALID
        /// </summary>
        Invalid = 0x00,
        /// <summary>
        /// e_MON_CMD_MEM_GET
        /// </summary>
        MemoryGet = 0x01,
        /// <summary>
        /// e_MON_CMD_MEM_SET
        /// </summary>
        MemorySet = 0x02,
        /// <summary>
        /// e_MON_CMD_CHECKPOINT_GET
        /// </summary>
        CheckpointGet = 0x11,
        /// <summary>
        /// e_MON_CMD_CHECKPOINT_SET
        /// </summary>
        CheckpointSet = 0x12,
        /// <summary>
        /// e_MON_CMD_CHECKPOINT_DELETE
        /// </summary>
        CheckpointDelete = 0x13,
        /// <summary>
        /// e_MON_CMD_CHECKPOINT_LIST
        /// </summary>
        CheckpointList = 0x14,
        /// <summary>
        /// e_MON_CMD_CHECKPOINT_TOGGLE
        /// </summary>
        CheckpointToggle = 0x15,
        /// <summary>
        /// e_MON_CMD_CONDITION_SET
        /// </summary>
        ConditionSet = 0x22,
        /// <summary>
        /// e_MON_CMD_REGISTERS_GET
        /// </summary>
        RegistersGet = 0x31,
        /// <summary>
        /// e_MON_CMD_REGISTERS_SET
        /// </summary>
        RegistersSet = 0x32,
        /// <summary>
        /// e_MON_CMD_DUMP
        /// </summary>
        Dump = 0x41,
        /// <summary>
        /// e_MON_CMD_UNDUMP
        /// </summary>
        Undump = 0x42,
        /// <summary>
        /// e_MON_CMD_RESOURCE_GET
        /// </summary>
        ResourceGet = 0x51,
        /// <summary>
        /// e_MON_CMD_RESOURCE_SET
        /// </summary>
        ResourceSet = 0x52,
        /// <summary>
        /// e_MON_CMD_ADVANCE_INSTRUCTIONS
        /// </summary>
        AdvanceInstruction = 0x71,
        /// <summary>
        /// e_MON_CMD_KEYBOARD_FEED
        /// </summary>
        KeyboardFeed = 0x72,
        /// <summary>
        /// e_MON_CMD_EXECUTE_UNTIL_RETURN
        /// </summary>
        ExecuteUntilReturn = 0x73,
        /// <summary>
        /// e_MON_CMD_PING
        /// </summary>
        Ping = 0x81,
        /// <summary>
        /// e_MON_CMD_BANKS_AVAILABLE
        /// </summary>
        BanksAvailable = 0x82,
        /// <summary>
        /// e_MON_CMD_REGISTERS_AVAILABLE
        /// </summary>
        RegistersAvailable = 0x83,
        /// <summary>
        /// e_MON_CMD_DISPLAY_GET
        /// </summary>
        DisplayGet = 0x84,
        /// <summary>
        /// e_MON_CMD_VICE_INFO
        /// </summary>
        Info = 0x85,
        /// <summary>
        /// e_MON_CMD_EXIT
        /// </summary>
        Exit = 0xaa,
        /// <summary>
        /// e_MON_CMD_QUIT
        /// </summary>
        Quit = 0xbb,
        /// <summary>
        /// e_MON_CMD_RESET
        /// </summary>
        Reset = 0xcc,
        /// <summary>
        /// e_MON_CMD_AUTOSTART
        /// </summary>
        AutoStart = 0xdd,
    }
}

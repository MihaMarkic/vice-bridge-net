namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// VICE's response codes
    /// </summary>
    public enum ErrorCode : byte
    {
        /// <summary>
        /// e_MON_ERR_OK
        /// </summary>
        OK = 0x00,
        /// <summary>
        /// e_MON_ERR_OBJECT_MISSING
        /// </summary>
        ObjectDoesNotExist = 0x01,
        /// <summary>
        /// e_MON_ERR_INVALID_MEMSPACE
        /// </summary>
        InvalidMemSpace = 0x02,
        /// <summary>
        /// e_MON_ERR_CMD_INVALID_LENGTH
        /// </summary>
        IncorrectCommandLength = 0x80,
        /// <summary>
        /// e_MON_ERR_INVALID_PARAMETER
        /// </summary>
        InvalidParameterValue = 0x81,
        /// <summary>
        /// e_MON_ERR_CMD_INVALID_API_VERSION
        /// </summary>
        UnknownApiVersion = 0x82,
        /// <summary>
        /// e_MON_ERR_CMD_INVALID_TYPE
        /// </summary>
        UnknownCommandType = 0x83,
        /// <summary>
        /// e_MON_ERR_CMD_FAILURE
        /// </summary>
        GeneralFailure = 0x8f,

    }
}

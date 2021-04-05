namespace Righthand.ViceMonitor.Bridge.Shared
{
    /// <summary>
    /// VICE's resource type codes
    /// </summary>
    public enum ResourceType : byte
    {
        /// <summary>
        /// e_MON_RESOURCE_TYPE_STRING
        /// </summary>
        String = 0x00,
        /// <summary>
        /// e_MON_RESOURCE_TYPE_INT
        /// </summary>
        Integer = 0x01,
    }
}

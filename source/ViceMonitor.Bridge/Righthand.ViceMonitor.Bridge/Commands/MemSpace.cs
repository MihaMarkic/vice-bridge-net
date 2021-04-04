namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Describes which part of the computer you want to operate on. 
    /// </summary>
    public enum MemSpace: byte
    {
        /// <summary>
        /// Main memory
        /// </summary>
        MainMemory  = 0x00,
        /// <summary>
        /// Drive 8
        /// </summary>
        Drive8      = 0x01,
        /// <summary>
        /// Drive 9
        /// </summary>
        Drive9      = 0x02,
        /// <summary>
        /// Drive 10
        /// </summary>
        Drive10     = 0x03,
        /// <summary>
        /// Drive 11
        /// </summary>
        Drive11     = 0x04,
    }
}

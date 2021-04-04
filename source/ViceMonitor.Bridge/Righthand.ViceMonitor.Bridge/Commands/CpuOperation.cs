namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// CPU Operation to execute
    /// </summary>
    public enum CpuOperation : byte
    {
        /// <summary>
        /// Load
        /// </summary>
        Load = 0x01,
        /// <summary>
        /// Store
        /// </summary>
        Store = 0x02,
        /// <summary>
        /// Exec
        /// </summary>
        Exec = 0x04,
    }
}

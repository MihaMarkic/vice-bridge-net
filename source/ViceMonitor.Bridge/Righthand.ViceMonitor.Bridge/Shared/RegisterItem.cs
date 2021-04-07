namespace Righthand.ViceMonitor.Bridge.Shared
{
    /// <summary>
    /// Represents a VICE register item
    /// </summary>
    /// <param name="RegisterId"></param>
    /// <param name="RegisterValue"></param>
    public record RegisterItem(byte RegisterId, ushort RegisterValue)
    {
        /// <summary>
        /// Size of the register.
        /// </summary>
        public const byte Size = 3;
        /// <summary>
        /// Register content length.
        /// </summary>
        public const uint ContentLength = 4;
    }
}

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Communication constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Message start. ASC_STX
        /// </summary>
        public const byte STX = 0x02;
        /// <summary>
        /// Event is unbound. MON_EVENT_ID
        /// </summary>
        public const uint BroadcastRequestId = 0xffffffff;

    }
}

namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Image format
    /// </summary>
    public enum ImageFormat: byte
    {
        /// <summary>
        /// Indexed, 8 bit
        /// </summary>
        Indexed = 0x00,
        /// <summary>
        /// RGB, 24 bit
        /// </summary>
        Rgb = 0x01,
        /// <summary>
        /// BGR, 24 bit
        /// </summary>
        Bgr = 0x02,
        /// <summary>
        /// RGBA, 32 bit
        /// </summary>
        Rgba = 0x03,
        /// <summary>
        /// BGRA, 32 bit
        /// </summary>
        Bgra = 0x04,
    }
}

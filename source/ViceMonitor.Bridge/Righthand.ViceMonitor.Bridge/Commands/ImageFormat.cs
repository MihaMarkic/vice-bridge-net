namespace Righthand.ViceMonitor.Bridge.Commands
{
    /// <summary>
    /// Image format
    /// </summary>
    public enum ImageFormat: byte
    {
        /// <summary>
        /// Indexed, 8 bit e_DISPLAY_GET_MODE_INDEXED8
        /// </summary>
        Indexed = 0x00,
        /// <summary>
        /// RGB, 24 bit e_DISPLAY_GET_MODE_RGB24
        /// </summary>
        Rgb = 0x01,
        /// <summary>
        /// BGR, 24 bit e_DISPLAY_GET_MODE_BGR24
        /// </summary>
        Bgr = 0x02,
        /// <summary>
        /// RGBA, 32 bit e_DISPLAY_GET_MODE_RGBA32
        /// </summary>
        Rgba = 0x03,
        /// <summary>
        /// BGRA, 32 bit e_DISPLAY_GET_MODE_BGRA32
        /// </summary>
        Bgra = 0x04,
    }
}

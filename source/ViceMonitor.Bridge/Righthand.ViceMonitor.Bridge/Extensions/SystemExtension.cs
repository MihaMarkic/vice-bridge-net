namespace System
{
    internal static class SystemExtension
    {
        internal static byte AsByte(this bool value) => value ? (byte)1 : (byte)0;
    }
}

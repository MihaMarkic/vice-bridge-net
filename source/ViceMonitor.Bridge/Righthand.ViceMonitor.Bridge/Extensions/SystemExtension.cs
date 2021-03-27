namespace System
{
    public static class SystemExtension
    {
        public static byte AsByte(this bool value) => value ? (byte)1 : (byte)0;
    }
}

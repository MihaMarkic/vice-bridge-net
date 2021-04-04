namespace System.IO
{
    internal static class BinaryWriterExtension
    {
        internal static void WriteBoolAsByte(this BinaryWriter writer, bool value)
        {
            writer.Write((byte)(value ? 1 : 0));
        }
    }
}

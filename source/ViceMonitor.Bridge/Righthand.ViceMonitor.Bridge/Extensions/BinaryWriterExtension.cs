namespace System.IO
{
    public static class BinaryWriterExtension
    {
        public static void WriteBoolAsByte(this BinaryWriter writer, bool value)
        {
            writer.Write((byte)(value ? 1 : 0));
        }
    }
}

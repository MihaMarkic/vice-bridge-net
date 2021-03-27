using Righthand.ViceMonitor.Bridge.Commands;

namespace System.IO
{
    public static class BinaryReaderExtension
    {
        public static bool ReadBoolFromByte(this BinaryReader reader)
        {
            return reader.ReadByte() == 1;
        }
        public static T ReadEnum<T>(this BinaryReader reader)
            where T: struct, Enum
        {
            // TODO could optimize and avoid boxing?
            // a suggestion: return Unsafe.As<int, TEnum>(ref int32);
            return (T)(object)reader.ReadByte();
        }
        public static CpuOperation ReadCpuOperation(this BinaryReader reader)
        {
            return (CpuOperation)reader.ReadByte();
        }
    }
}

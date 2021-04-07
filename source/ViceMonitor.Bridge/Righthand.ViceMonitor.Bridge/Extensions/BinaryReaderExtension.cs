using Righthand.ViceMonitor.Bridge.Commands;

namespace System.IO
{
    internal static class BinaryReaderExtension
    {
        internal static bool ReadBoolFromByte(this BinaryReader reader)
        {
            return reader.ReadByte() == 1;
        }
        internal static T ReadEnum<T>(this BinaryReader reader)
            where T: struct, Enum
        {
            // TODO could optimize and avoid boxing?
            // a suggestion: return Unsafe.As<int, TEnum>(ref int32);
            return (T)(object)reader.ReadByte();
        }
        internal static CpuOperation ReadCpuOperation(this BinaryReader reader)
        {
            return (CpuOperation)reader.ReadByte();
        }
    }
}

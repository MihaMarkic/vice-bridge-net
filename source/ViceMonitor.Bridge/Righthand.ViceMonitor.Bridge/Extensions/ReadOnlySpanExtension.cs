namespace System;

/// <summary>
/// Adds <see cref="ReadOnlySpan{T}"/> data read functions. 
/// </summary>
internal static class ReadOnlySpanExtension
{
	internal static UInt64 ReadUInt64(this ref ReadOnlySpan<byte> buffer)
	{
		UInt64 result = BitConverter.ToUInt64(buffer);
		buffer = buffer[sizeof(UInt64)..];
		return result;
	}
	internal static UInt32 ReadUInt32(this ref ReadOnlySpan<byte> buffer)
	{
		UInt32 result = BitConverter.ToUInt32(buffer);
		buffer = buffer[sizeof(UInt32)..];
		return result;
	}
	internal static UInt16 ReadUInt16(this ref ReadOnlySpan<byte> buffer)
	{
		UInt16 result = BitConverter.ToUInt16(buffer);
		buffer = buffer[sizeof(UInt16)..];
		return result;
	}
	internal static byte ReadByte(this ref ReadOnlySpan<byte> buffer)
	{
		byte result = buffer[0];
		buffer = buffer[sizeof(byte)..];
		return result;
	}
	internal static ReadOnlySpan<byte> ReadBytes(this ref ReadOnlySpan<byte> buffer, int length)
	{
		var result = buffer[0..length];
		buffer = buffer[length..];
		return result;
	}
}
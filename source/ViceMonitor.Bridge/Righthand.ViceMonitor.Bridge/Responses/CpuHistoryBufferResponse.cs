using System.Diagnostics;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Shared;

namespace Righthand.ViceMonitor.Bridge.Responses;

/// <summary>
/// Response MON_RESPONSE_CPUHISTORY_GET to <see cref="CpuHistoryCommand"/> as buffer.
/// </summary>
/// <param name="ApiVersion"></param>
/// <param name="ErrorCode"></param>
/// <param name="Buffer"></param>
/// <remarks>
/// Object has to be disposed once it is processed to free memory occupied by <see cref="Buffer"/>.
/// </remarks>
public record CpuHistoryBufferResponse(byte ApiVersion, ErrorCode ErrorCode, ManagedBuffer? Buffer)
	: ViceResponse(ApiVersion, ErrorCode)
{
	/// <summary>
	/// Parses buffer and returns an array of <see cref="CpuHistoryItem"/>. 
	/// </summary>
	/// <returns></returns>
	public  ImmutableArray<CpuHistoryItem> ParseContent()
	{
		if (Buffer is null)
		{
			return [];
		}
		
		ReadOnlySpan<byte> buffer = Buffer.Value.Data.AsSpan();
		return BuildCpuHistoryResponse(buffer);
	}

	/// <summary>
	/// Checks for instruction at the given address.
	/// </summary>
	/// <param name="address"></param>
	/// <param name="pcRegisterIndex">PC register index.</param>
	/// <returns></returns>
	/// <remarks>Useful to quickly find return address when checking program for termination.</remarks>
	public bool CheckForInstructionAtAddress(ushort address, ushort pcRegisterIndex)
	{
		if (ErrorCode != ErrorCode.OK || Buffer is null)
		{
			return false;
		}
		
		ReadOnlySpan<byte> buffer = Buffer.Value.Data.AsSpan();
		uint itemCount = buffer.ReadUInt32();
		for (uint i = 0; i < itemCount; i++)
		{
			if (CheckForInstructionAtAddress(ref buffer, address, pcRegisterIndex))
			{
				return true;
			}
		}

		return false;
	}

	internal static bool CheckForInstructionAtAddress(ref ReadOnlySpan<byte> buffer, ushort address, ushort pcRegisterIndex)
	{
		if (buffer.Length < 1)
		{
			throw new Exception("Empty buffer, nothing to read from");
		}
		var itemSize = buffer.ReadByte();
		if (buffer.Length < itemSize)
		{
			throw new Exception("Buffer is too small");
		}
		
		ushort registerItemsCount = buffer.ReadUInt16();

		// check the value of PC register
		for (ushort i = 0; i < registerItemsCount; i++)
		{
			var length = buffer.ReadByte();
			Debug.Assert(length == 3);
			//Size: itemBuffer[0], should be 3
			var registerId = buffer.ReadByte();
			var registerValue = buffer.ReadUInt16();
			if (registerId == pcRegisterIndex && registerValue == address)
			{
				return true;
			}
		}
		// skip the rest
		_ = buffer.ReadUInt64();
		var instructionDataLength = buffer.ReadByte();
		_ = buffer.ReadBytes(instructionDataLength);

		return false;
	}
	
	/// <summary>
	/// Provides parsing for CPU history response.
	/// </summary>
	/// <param name="buffer"></param>
	/// <returns></returns>
	internal ImmutableArray<CpuHistoryItem> BuildCpuHistoryResponse(ReadOnlySpan<byte> buffer)
	{
		uint itemCount = buffer.ReadUInt32();
		var items = new CpuHistoryItem[itemCount];
		for (uint i = 0; i < itemCount; i++)
		{
			items[i] = BuildCpuHistoryItem(ref buffer);								
		}

		return [.. items];
	}
	internal static CpuHistoryItem BuildCpuHistoryItem(ref ReadOnlySpan<byte> buffer)
	{
		var itemSize = buffer.ReadByte();
		if (buffer.Length < itemSize)
		{
			throw new Exception("Buffer is too small");
		}
		var registerItems = RegistersResponse.BuildRegisterItems(ref buffer);
		var cpuClock = buffer.ReadUInt64();
		var instructionDataLength = buffer.ReadByte();
		ReadOnlySpan<byte> instructionData = buffer.ReadBytes(instructionDataLength);
		return new CpuHistoryItem([..registerItems], cpuClock, [..instructionData]);
	}
	
	/// <summary>
	/// Releases all resources used by the <see cref="DisplayGetResponse"/>.
	/// </summary>
	public void Dispose()
	{
		Buffer?.Dispose();
	}
}
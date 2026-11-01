namespace Righthand.ViceMonitor.Bridge.Shared;

/// <summary>
/// Represents a VICE CPU history item. 
/// </summary>
/// <param name="RegisterItems"></param>
/// <param name="CpuClock"></param>
/// <param name="InstructionData"></param>
public record CpuHistoryItem(
	ImmutableArray<RegisterItem> RegisterItems,
	ulong CpuClock,
	ImmutableArray<byte> InstructionData)
{
	/// <summary>
	/// Gets address of this instruction.
	/// </summary>
	/// <param name="addressRegisterId"></param>
	/// <returns></returns>
	public ushort? GetAddress(byte addressRegisterId)
	{
		var addressRegisterItem = RegisterItems.SingleOrDefault(i => i.RegisterId == addressRegisterId);
		return addressRegisterItem?.RegisterValue;
	}
}
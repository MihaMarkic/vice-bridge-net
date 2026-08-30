namespace Righthand.ViceMonitor.Bridge.Shared;

/// <summary>
/// Represents a VICE CPU history item. 
/// </summary>
/// <param name="RegisterItems"></param>
/// <param name="CpuClock"></param>
/// <param name="InstructionData"></param>
public record CpuHistoryItem(ImmutableArray<RegisterItem> RegisterItems, ulong CpuClock, ImmutableArray<byte> InstructionData);
using Righthand.ViceMonitor.Bridge.Responses;

namespace Righthand.ViceMonitor.Bridge.Commands;

/// <summary>
/// Gets records of every instruction executed by an emulated CPU.
/// </summary>
/// <param name="MemSpace"></param>
/// <param name="ItemsCount"></param>
/// <remarks>Minimum VICE version: 3.10</remarks>
public record CpuHistoryCommand(MemSpace MemSpace, uint ItemsCount) : ViceCommand<CpuHistoryResponse>(CommandType.CpuHistory)
{
	/// <inheritdoc />
	public override uint ContentLength { get; } = sizeof(byte) + sizeof(uint);
	/// <inheritdoc/>
	public override void WriteContent(Span<byte> buffer)
	{
		buffer[0] = (byte)MemSpace;
		BitConverter.TryWriteBytes(buffer[1..], ItemsCount);
	}
}
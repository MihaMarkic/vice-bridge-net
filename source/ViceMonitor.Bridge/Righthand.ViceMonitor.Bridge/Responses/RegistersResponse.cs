using System.Diagnostics;
using Righthand.ViceMonitor.Bridge.Commands;
using Righthand.ViceMonitor.Bridge.Shared;

namespace Righthand.ViceMonitor.Bridge.Responses;

/// <summary>
/// Response to <see cref="Righthand.ViceMonitor.Bridge.Commands.RegistersGetCommand"/> and 
/// <see cref="Righthand.ViceMonitor.Bridge.Commands.RegistersSetCommand"/>.
/// </summary>
/// <param name="ApiVersion"><inheritdoc /></param>
/// <param name="ErrorCode"><inheritdoc /></param>
/// <param name="Items">
/// An array with items of structure:
///   byte 0: Size of the item, excluding this byte
///   byte 1: ID of the register
///   byte 2-3: register value
/// </param>
public record RegistersResponse(byte ApiVersion, ErrorCode ErrorCode, ImmutableArray<RegisterItem> Items)
	: ViceResponse(ApiVersion, ErrorCode)
{
	internal static ImmutableArray<RegisterItem> BuildRegisterItems(ref ReadOnlySpan<byte> buffer)
	{
		ushort itemsCount = buffer.ReadUInt16();
		var items = new RegisterItem[itemsCount];
		for (ushort i = 0; i < itemsCount; i++)
		{
			var length = buffer.ReadByte();
			Debug.Assert(length == 3);
			var item = new RegisterItem(
				//Size: itemBuffer[0], should be 3
				RegisterId: buffer.ReadByte(),
				RegisterValue: buffer.ReadUInt16()
			);
			items[i] = item;
		}

		return [..items];
	}
}
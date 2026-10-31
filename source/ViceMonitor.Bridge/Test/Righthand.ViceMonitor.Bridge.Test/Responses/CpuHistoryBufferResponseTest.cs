using NUnit.Framework;
using Righthand.ViceMonitor.Bridge.Responses;
using Righthand.ViceMonitor.Bridge.Test.TestExtensions;

namespace Righthand.ViceMonitor.Bridge.Test.Responses;

[TestFixture]
public class CpuHistoryBufferResponseTest: BaseTest<CpuHistoryBufferResponse>
{
	public class CheckForInstructionAtAddress : CpuHistoryBufferResponseTest
	{
		[TestCase("09 02 00 03 00 01 02 03 03 00 f0", 0xf000, ExpectedResult = true)]
		public bool Test(string data, int address)
		{
			ReadOnlySpan<byte> bytes = data.ToHexBytes();
			return CpuHistoryBufferResponse.CheckForInstructionAtAddress(ref bytes, (ushort)address, 3);
		}
	}
}
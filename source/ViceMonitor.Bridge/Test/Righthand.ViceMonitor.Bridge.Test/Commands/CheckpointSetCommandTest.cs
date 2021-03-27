using System;
using NUnit.Framework;
using Righthand.ViceMonitor.Bridge.Commands;

namespace Righthand.ViceMonitor.Bridge.Test.Commands
{
    class CheckpointSetCommandTest: BaseTest<CheckpointSetCommand>
    {
        [TestFixture]
        public class ContentLength: CheckpointSetCommandTest
        {
            [Test]
            public void ShouldBeAlwaysEight()
            {
                var command = new CheckpointSetCommand(0xfce2, 0xfce3,
                    StopWhenHit: true, Enabled: true, CpuOperation.Exec, Temporary: true);

                Assert.That(command.ContentLength, Is.EqualTo(8));
            }
        }
        [TestFixture]
        public class GetBinaryData : CheckpointSetCommandTest
        {
            // https://vice-emu.sourceforge.io/vice_toc.html#TOC284
            [Test]
            public void HeaderIsWrittenCorrectly()
            {
                var command = new CheckpointSetCommand(0xfce2, 0xfce3,
                    StopWhenHit: true, Enabled: true, CpuOperation.Exec, Temporary: true);

                (var buffer, uint length) = command.GetBinaryData(0x0123);
                try
                {
                    ReadOnlySpan<byte> data = buffer.Data.AsSpan();
                    // header + content
                    Assert.That(length, Is.EqualTo(11 + 8));
                    Assert.That(data[0], Is.EqualTo(Constants.STX));
                    Assert.That(data[1], Is.EqualTo(0x01));
                    // body length
                    Assert.That(BitConverter.ToUInt32(data[2..]), Is.EqualTo(8));
                    Assert.That(BitConverter.ToUInt32(data[6..]), Is.EqualTo(0x0123));
                    Assert.That(data[10], Is.EqualTo((byte)CommandType.CheckpointSet));
                }
                finally
                {
                    buffer.Dispose();
                }
            }
            // https://vice-emu.sourceforge.io/vice_toc.html#TOC284
            [Test]
            public void ContentIsWrittenCorrectly()
            {
                var command = new CheckpointSetCommand(0xfce2, 0xfce3,
                    StopWhenHit: true, Enabled: true, CpuOperation.Exec, Temporary: true);

                (var buffer, uint length) = command.GetBinaryData(0x0123);
                try
                {
                    ReadOnlySpan<byte> data = buffer.Data.AsSpan();
                    var target = GetByteArrayFromData("e2 fc | e3 fc | 01 | 01 | 04 | 01");
                    Assert.That(data.Slice(11, target.Length).ToArray(), Is.EqualTo(target));
                }
                finally
                {
                    buffer.Dispose();
                }
            }
        }
        [TestFixture]
        public class WriteContent: CheckpointSetCommandTest
        {
            [Test]
            public void GivenSampleData_WritesDataCorrectly()
            {
                var command = new CheckpointSetCommand(0xfce2, 0xfce3, 
                    StopWhenHit: true, Enabled : true, CpuOperation.Exec, Temporary: true);
                var buffer = new byte[command.ContentLength];
                {
                    command.WriteContent(buffer);
                }
                Assert.That(buffer, Is.EqualTo(GetByteArrayFromData("e2 fc | e3 fc | 01 | 01 | 04 | 01")));
            }
        }
    }
}

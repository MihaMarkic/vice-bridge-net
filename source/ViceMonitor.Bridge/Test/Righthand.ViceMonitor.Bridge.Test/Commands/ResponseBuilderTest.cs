using System;
using NUnit.Framework;
using Righthand.ViceMonitor.Bridge.Commands;

namespace Righthand.ViceMonitor.Bridge.Test.Commands
{
    class ResponseBuilderTest: BaseTest<ResponseBuilder>
    {
        [TestFixture]
        public class BuildCheckpointSetResponse : ResponseBuilderTest
        {
            /// <summary>
            /// Test based on sample from VICE manual https://vice-emu.sourceforge.io/vice_toc.html#TOC284
            /// </summary>
            [Test]
            public void GivenSampleData_CreatesCorrectResponse()
            {
                ReadOnlySpan<byte> data = GetByteArrayFromData("02 00 00 00 | 00 | e2 fc | e3 fc | 01 | 01 | 04 | 01 | 00 00 00 00 | 00 00 00 00 | 00").AsSpan();
                var actual = Target.BuildCheckpointSetResponse(0x01, ErrorCode.OK, data);

                Assert.That(actual.ApiVersion, Is.EqualTo(0x01));
                Assert.That(actual.ErrorCode, Is.EqualTo(ErrorCode.OK));
                Assert.That(actual.CheckpointNumber, Is.EqualTo(0x02));
                Assert.That(actual.CurrentlyHit, Is.False);
                Assert.That(actual.StartAddress, Is.EqualTo(0xfce2));
                Assert.That(actual.EndAddress, Is.EqualTo(0xfce3));
                Assert.That(actual.StopWhenHit, Is.True);
                Assert.That(actual.Enabled, Is.True);
                Assert.That(actual.CpuOperation, Is.EqualTo(CpuOperation.Exec));
                Assert.That(actual.Temporary, Is.True);
                Assert.That(actual.HitCount, Is.EqualTo(0x00000000));
                Assert.That(actual.IgnoreCount, Is.EqualTo(0x00000000));
                Assert.That(actual.HasCondition, Is.False);
            }
        }
    }
}

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NUnit.Framework;

namespace Righthand.ViceMonitor.Bridge.Test
{
    public abstract class BaseTest<T>: BaseTest
        where T : class
    {
        protected Fixture fixture;
        T target;
        public T Target
        {
            [DebuggerStepThrough]
            get
            {
                if (target is null)
                {
                    target = fixture.Build<T>().OmitAutoProperties().Create();
                }
                return target;
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoNSubstituteCustomization());
        }
        [TearDown]
        public void TearDown()
        {
            target = null;
        }
    }

    public class BaseTest
    {
        protected byte[] GetByteArrayFromData(string data)
        {
            string one = data.Replace("| ", "");
            byte[] parts = one
                .Split(' ')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => byte.Parse(s, NumberStyles.HexNumber))
                .ToArray();
            return parts;
        }
    }
}

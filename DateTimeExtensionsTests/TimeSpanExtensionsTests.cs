using DateTimeExtensions;
using NUnit.Framework;

namespace DateTimeExtensionsTests
{
    public class TimeSpanExtensionsTests
    {
        #region Public Methods

        [Test]
        public void GetTimespanWithComma()
        {
            const string time1 = "0,25399";
            var result1 = time1.ToTimeSpan();

            Assert.IsTrue(result1.Value.Hours == 6);
            Assert.IsTrue(result1.Value.Minutes == 5);
            Assert.IsTrue(result1.Value.Seconds == 44);

            const string time2 = "0.25399";
            var result2 = time2.ToTimeSpan();

            Assert.IsTrue(result2.Value.Hours == 6);
            Assert.IsTrue(result2.Value.Minutes == 5);
            Assert.IsTrue(result2.Value.Seconds == 44);
        }

        [Test]
        public void GetTimespanWithDelimiters()
        {
            const string time1 = "10.05.18";
            var result1 = time1.ToTimeSpan(".");

            Assert.IsTrue(result1.Value.Hours == 10);
            Assert.IsTrue(result1.Value.Minutes == 5);
            Assert.IsTrue(result1.Value.Seconds == 18);

            const string time2 = "13\\20\\55";
            var result2 = time2.ToTimeSpan("\\");

            Assert.IsTrue(result2.Value.Hours == 13);
            Assert.IsTrue(result2.Value.Minutes == 20);
            Assert.IsTrue(result2.Value.Seconds == 55);
        }

        #endregion Public Methods
    }
}
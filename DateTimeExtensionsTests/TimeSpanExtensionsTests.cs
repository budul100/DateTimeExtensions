using DateTimeExtensions;
using NUnit.Framework;

namespace DateTimeExtensionsTests
{
    public class TimeSpanExtensionsTests
    {
        #region Public Methods

        [Test]
        public void GetTimespanWithDelimiters()
        {
            var time1 = "10.05.18";
            var result1 = time1.ToTimeSpan(".");

            Assert.IsTrue(result1.Value.Hours == 10);
            Assert.IsTrue(result1.Value.Minutes == 5);
            Assert.IsTrue(result1.Value.Seconds == 18);

            var time2 = "13\\20\\55";
            var result2 = time2.ToTimeSpan("\\");

            Assert.IsTrue(result2.Value.Hours == 13);
            Assert.IsTrue(result2.Value.Minutes == 20);
            Assert.IsTrue(result2.Value.Seconds == 55);
        }

        #endregion Public Methods
    }
}
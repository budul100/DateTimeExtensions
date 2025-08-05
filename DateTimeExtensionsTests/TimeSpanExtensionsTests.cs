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

            Assert.That(result1.Value.Hours == 6);
            Assert.That(result1.Value.Minutes == 5);
            Assert.That(result1.Value.Seconds == 44);

            const string time2 = "0.25399";
            var result2 = time2.ToTimeSpan();

            Assert.That(result2.Value.Hours == 6);
            Assert.That(result2.Value.Minutes == 5);
            Assert.That(result2.Value.Seconds == 44);
        }

        [Test]
        public void GetTimespanWithDelimiters()
        {
            const string time1 = "10.05.18";
            var result1 = time1.ToTimeSpan(".");

            Assert.That(result1.Value.Hours == 10);
            Assert.That(result1.Value.Minutes == 5);
            Assert.That(result1.Value.Seconds == 18);

            const string time2 = "13\\20\\55";
            var result2 = time2.ToTimeSpan("\\");

            Assert.That(result2.Value.Hours == 13);
            Assert.That(result2.Value.Minutes == 20);
            Assert.That(result2.Value.Seconds == 55);
        }

        [Test]
        public void GetTimespanWithoutDelimiters()
        {
            const string time1 = "100518";
            var result1 = time1.ToTimeSpan();

            Assert.That(result1.Value.Hours == 10);
            Assert.That(result1.Value.Minutes == 5);
            Assert.That(result1.Value.Seconds == 18);

            const string time2 = "132055";
            var result2 = time2.ToTimeSpan();

            Assert.That(result2.Value.Hours == 13);
            Assert.That(result2.Value.Minutes == 20);
            Assert.That(result2.Value.Seconds == 55);

            const string time3 = "0408";
            var result3 = time3.ToTimeSpan();

            Assert.That(result3.Value.Hours == 4);
            Assert.That(result3.Value.Minutes == 8);
            Assert.That(result3.Value.Seconds == 0);

            const string time4 = "2255";
            var result4 = time4.ToTimeSpan();

            Assert.That(result4.Value.Hours == 22);
            Assert.That(result4.Value.Minutes == 55);
            Assert.That(result4.Value.Seconds == 0);
        }

        #endregion Public Methods
    }
}
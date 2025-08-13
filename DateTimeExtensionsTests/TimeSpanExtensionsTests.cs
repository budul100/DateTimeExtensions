using DateTimeExtensions;
using Xunit;

namespace DateTimeExtensionsTests
{
    public class TimeSpanExtensionsTests
    {
        #region Public Methods

        [Fact]
        public void GetTimespanWithComma()
        {
            const string time1 = "0,25399";
            var result1 = time1.ToTimeSpan();
            Assert.Equal(6, result1.Value.Hours);
            Assert.Equal(5, result1.Value.Minutes);
            Assert.Equal(44, result1.Value.Seconds);

            const string time2 = "0.25399";
            var result2 = time2.ToTimeSpan();
            Assert.Equal(6, result2.Value.Hours);
            Assert.Equal(5, result2.Value.Minutes);
            Assert.Equal(44, result2.Value.Seconds);
        }

        [Fact]
        public void GetTimespanWithDelimiters()
        {
            const string time1 = "10.05.18";
            var result1 = time1.ToTimeSpan(".");
            Assert.Equal(10, result1.Value.Hours);
            Assert.Equal(5, result1.Value.Minutes);
            Assert.Equal(18, result1.Value.Seconds);

            const string time2 = "13\\20\\55";
            var result2 = time2.ToTimeSpan("\\");
            Assert.Equal(13, result2.Value.Hours);
            Assert.Equal(20, result2.Value.Minutes);
            Assert.Equal(55, result2.Value.Seconds);
        }

        [Fact]
        public void GetTimespanWithoutDelimiters()
        {
            const string time1 = "100518";
            var result1 = time1.ToTimeSpan();
            Assert.Equal(10, result1.Value.Hours);
            Assert.Equal(5, result1.Value.Minutes);
            Assert.Equal(18, result1.Value.Seconds);

            const string time2 = "132055";
            var result2 = time2.ToTimeSpan();
            Assert.Equal(13, result2.Value.Hours);
            Assert.Equal(20, result2.Value.Minutes);
            Assert.Equal(55, result2.Value.Seconds);

            const string time3 = "0408";
            var result3 = time3.ToTimeSpan();
            Assert.Equal(4, result3.Value.Hours);
            Assert.Equal(8, result3.Value.Minutes);
            Assert.Equal(0, result3.Value.Seconds);

            const string time4 = "2255";
            var result4 = time4.ToTimeSpan();
            Assert.Equal(22, result4.Value.Hours);
            Assert.Equal(55, result4.Value.Minutes);
            Assert.Equal(0, result4.Value.Seconds);
        }

        #endregion Public Methods
    }
}
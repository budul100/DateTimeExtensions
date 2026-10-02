using System;
using System.Globalization;
using DateTimeExtensions;
using Xunit;

namespace DateTimeExtensionsTests
{
    public class TimeSpanExtensionsTests
    {
        #region Public Methods

        [Fact]
        public void GetTimespanWithAM()
        {
            const string time1 = "12:02:30 AM";
            var result1 = time1.ToTimeSpan();
            Assert.Equal(0, result1.Value.Hours);
            Assert.Equal(2, result1.Value.Minutes);
            Assert.Equal(30, result1.Value.Seconds);

            const string time2 = "12:02:30 PM";
            var result2 = time2.ToTimeSpan();
            Assert.Equal(12, result2.Value.Hours);
            Assert.Equal(2, result2.Value.Minutes);
            Assert.Equal(30, result2.Value.Seconds);
        }

        [Fact]
        public void GetTimespanWithComma()
        {
            var previous = CultureInfo.CurrentCulture;

            try
            {
                // Comma as decimal separator requires a matching culture
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var result1 = "0,25399".ToTimeSpan();
                Assert.Equal(new TimeSpan(6, 5, 44), result1.Value
                    .Add(TimeSpan.FromMilliseconds(-result1.Value.Milliseconds)));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }

            // Invariant format works independently of the current culture
            var result2 = "0.25399".ToTimeSpan();
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

        [Theory]
        [InlineData("12:02:30 AM", 0, 2, 30)]
        [InlineData("12:02:30 PM", 12, 2, 30)]
        [InlineData("1:15 p.m.", 13, 15, 0)]
        [InlineData("0115PM", 13, 15, 0)]
        [InlineData("23:15", 23, 15, 0)]
        [InlineData("1.06:00", 30, 0, 0)] // day prefix
        [InlineData("06:00[+1]", 30, 0, 0)] // day suffix
        public void ToTimeSpanParsesClockFormats(string input, int hours, int minutes, int seconds)
        {
            var expected = new TimeSpan(hours, minutes, seconds);

            Assert.Equal(expected, input.ToTimeSpan());
        }

        [Theory]
        [InlineData("13:00 PM")]
        [InlineData("0:30 AM")]
        [InlineData("")]
        [InlineData("abc")]
        public void ToTimeSpanRejectsInvalidInput(string input)
        {
            Assert.Null(input.ToTimeSpan());
        }

        [Fact]
        public void ToTimeSpanWithDelimitersAndMeridiem()
        {
            Assert.Equal(new TimeSpan(22, 5, 18), "10.05.18 PM".ToTimeSpan("."));
        }

        #endregion Public Methods
    }
}
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
            // Culture data is missing in invariant globalization mode (e.g. slim containers)
            Assert.SkipWhen(
                IsInvariantGlobalization(),
                "A de-DE culture is not available in invariant globalization mode.");

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
        [InlineData(" 13:20 ", 13, 20, 0)] // surrounding whitespace
        [InlineData("25:30", 25, 30, 0)] // service days beyond midnight
        [InlineData("1.5", 36, 0, 0)] // fraction of a day
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
        [InlineData("2020-01-10")] // no embedded time
        [InlineData("123456789")] // too long
        [InlineData("1234567")] // too long
        [InlineData("12:75")] // minutes out of range
        [InlineData("12:30:61")] // seconds out of range
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-0.25")]
        [InlineData("-1000000")]
        [InlineData("3000000.5")] // beyond the OLE Automation range
        [InlineData("١٢:٣٠")] // non ASCII digits
        public void ToTimeSpanRejectsInvalidInput(string input)
        {
            Assert.Null(input.ToTimeSpan());
        }

        [Fact]
        public void ToTimeSpanWithDelimitersAndMeridiem()
        {
            Assert.Equal(new TimeSpan(22, 5, 18), "10.05.18 PM".ToTimeSpan("."));
        }

        [Fact]
        public void ToTimeSpanWithDelimitersIsNoDayFraction()
        {
            // Without delimiters "10.05" would be a number, with them it is a time
            Assert.Equal(new TimeSpan(10, 5, 0), "10.05".ToTimeSpan("."));
        }

        [Fact]
        public void ToTimeSpanWithSpecialCharactersInDelimiters()
        {
            // A '-' inside the delimiters must not form a character range
            Assert.Equal(new TimeSpan(10, 5, 0), "10-05".ToTimeSpan(".-:"));
            Assert.Null("1005".ToTimeSpan(".-:"));
        }

        #endregion Public Methods

        #region Private Methods

        private static bool IsInvariantGlobalization()
        {
            return AppContext.TryGetSwitch(
                switchName: "System.Globalization.Invariant",
                isEnabled: out var enabled)
                && enabled;
        }

        #endregion Private Methods
    }
}

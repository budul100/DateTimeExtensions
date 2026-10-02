using DateTimeExtensions;
using System;
using Xunit;

namespace DateTimeExtensionsTests
{
    public class ValueExtensionsTests
    {
        #region Public Methods

        [Theory]
        [InlineData("1")]
        [InlineData("101")]
        [InlineData("0001000")]
        public void BitmaskRoundtrip(string bitmask)
        {
            // The length overload is required, otherwise trailing negative bits are lost
            var result = bitmask.GetBits().ToBitmask(
                length: bitmask.Length);

            Assert.Equal(bitmask, result);
        }

        [Fact]
        public void BitmaskWithoutPositiveBitsIsEmpty()
        {
            Assert.Equal(string.Empty, "0000000".GetBits().ToBitmask(length: 7));
            Assert.Null("0000000".GetBits().ToBitmask(length: 7, defaultOnEmpty: true));
        }

        [Theory]
        [InlineData(new[] { 0 }, "1")]
        [InlineData(new[] { 0, 2 }, "101")]
        [InlineData(new[] { 3 }, "0001")]
        public void GetBitmaskFromBits(int[] bits, string expected)
        {
            Assert.Equal(expected, bits.ToBitmask());
        }

        [Fact]
        public void GetBitmaskFromDates()
        {
            var dates = new DateTime[] { DateTime.Today, DateTime.Today.AddDays(2) };
            var result = dates.ToBitmask(
                positiveBit: 'Y',
                negativeBit: 'N');

            Assert.Equal("YNY", result);
        }

        [Fact]
        public void GetBitmaskFromDatesWithTimeOfDay()
        {
            var dates = new DateTime[] { new(2020, 1, 10, 10, 0, 0), new(2020, 1, 12, 8, 0, 0) };

            var result = dates.ToBitmask(
                positiveBit: 'Y',
                negativeBit: 'N');

            Assert.Equal("YNY", result);
        }

        [Fact]
        public void GetBitmaskFromUnorderedDuplicateDates()
        {
            var dates = new DateTime[] { new(2020, 1, 12), new(2020, 1, 10), new(2020, 1, 12) };

            Assert.Equal("101", dates.ToBitmask());
        }

        [Fact]
        public void GetEmptyBitmask()
        {
            var dates = Array.Empty<DateTime>();
            var resultWoDefault = dates.ToBitmask();
            Assert.True(string.IsNullOrEmpty(resultWoDefault));

            var resultWDefault = dates.ToBitmask(true);
            Assert.True(resultWDefault == default);
        }

        [Fact]
        public void ToTimeStringFormatsSignAndDays()
        {
            Assert.Equal("00:30:00", TimeSpan.FromMinutes(30).ToTimeString());
            Assert.Equal("-00:30:00", TimeSpan.FromMinutes(-30).ToTimeString());
            Assert.Null(default(TimeSpan?).ToTimeString());

            // Documents the current behaviour: days are dropped by "hh"
            Assert.Equal("06:00:00", new TimeSpan(1, 6, 0, 0).ToTimeString());

            Assert.Equal("30:00:00", new TimeSpan(1, 6, 0, 0).ToTotalTimeString());
            Assert.Equal("-25:10:00", new TimeSpan(-1, -1, -10, 0).ToTotalTimeString());
        }

        #endregion Public Methods
    }
}
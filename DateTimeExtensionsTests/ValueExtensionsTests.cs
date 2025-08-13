using System;
using DateTimeExtensions;
using Xunit;

namespace DateTimeExtensionsTests
{
    public class ValueExtensionsTests
    {
        #region Public Methods

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
        public void GetEmptyBitmask()
        {
            var dates = Array.Empty<DateTime>();
            var resultWoDefault = dates.ToBitmask();
            Assert.True(string.IsNullOrEmpty(resultWoDefault));

            var resultWDefault = dates.ToBitmask(true);
            Assert.True(resultWDefault == default);
        }

        #endregion Public Methods
    }
}
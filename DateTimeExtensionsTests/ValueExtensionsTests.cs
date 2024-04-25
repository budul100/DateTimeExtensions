using DateTimeExtensions;
using NUnit.Framework;
using System;

namespace DateTimeExtensionsTests
{
    public class ValueExtensionsTests
    {
        #region Public Methods

        [Test]
        public void GetBitmaskFromDates()
        {
            var dates = new DateTime[] { DateTime.Today, DateTime.Today.AddDays(2) };

            var result = dates.ToBitmask(
                positiveBit: 'Y',
                negativeBit: 'N');

            Assert.That(result == "YNY");
        }

        [Test]
        public void GetEmptyBitmask()
        {
            var dates = Array.Empty<DateTime>();

            var resultWoDefault = dates.ToBitmask();

            Assert.That(string.IsNullOrEmpty(resultWoDefault));

            var resultWDefault = dates.ToBitmask(true);

            Assert.That(resultWDefault == default);
        }

        #endregion Public Methods
    }
}
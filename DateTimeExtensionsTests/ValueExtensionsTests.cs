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

            var result = dates.ToBitmask();

            Assert.IsTrue(result == "101");
        }

        [Test]
        public void GetEmptyBitmask()
        {
            var dates = Array.Empty<DateTime>();

            var resultWoDefault = dates.ToBitmask();

            Assert.IsTrue(string.IsNullOrEmpty(resultWoDefault));

            var resultWDefault = dates.ToBitmask(true);

            Assert.IsTrue(resultWDefault == default);
        }

        #endregion Public Methods
    }
}
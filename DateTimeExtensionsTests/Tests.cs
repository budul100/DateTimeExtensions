using DateTimeExtensions;
using NUnit.Framework;
using System;
using System.Linq;

namespace DateTimeExtensionsTests
{
    public class Tests
    {
        #region Public Methods

        [Test]
        public void CycleDatesMultiDay()
        {
            var cycle = new DateTime[] { new DateTime(2020, 1, 10), new DateTime(2020, 1, 16) };

            var inCycle = new DateTime(2020, 1, 12).GetCyclic(cycle);
            Assert.True(inCycle == new DateTime(2020, 1, 12));

            var atBorder = new DateTime(2020, 1, 10).GetCyclic(cycle);
            Assert.True(atBorder == new DateTime(2020, 1, 10));

            var leftAtBorder = new DateTime(2020, 1, 2).GetCyclic(cycle);
            Assert.True(leftAtBorder == new DateTime(2020, 1, 16));

            var leftToCycle = new DateTime(2020, 1, 3).GetCyclic(cycle);
            Assert.True(leftToCycle == new DateTime(2020, 1, 10));

            var rightToCycle = new DateTime(2020, 1, 30).GetCyclic(cycle);
            Assert.True(rightToCycle == new DateTime(2020, 1, 16));
        }

        [Test]
        public void CycleDatesSingleDay()
        {
            var cycle = new DateTime[] { new DateTime(2020, 1, 10) };

            var outCycle = new DateTime(2020, 1, 12).GetCyclic(cycle);
            Assert.True(outCycle == new DateTime(2020, 1, 10));

            var inCycle = new DateTime(2020, 1, 10).GetCyclic(cycle);
            Assert.True(inCycle == new DateTime(2020, 1, 10));
        }

        [Test]
        public void GetMultipleDatesFromString()
        {
            var test = "2020-01-10,2020-01-12>2020-01-14,2020-01-16";

            var dates = test.GetDates();

            Assert.IsTrue(dates.Count() == 5);
        }

        [Test]
        public void GetMultipleDatesWithFromToSeparator()
        {
            var test = "2020-01-10;2020-01-12>2020-01-14;2020-01-16";

            Assert.Throws<ArgumentException>(() => test.GetDates(">"));
        }

        [Test]
        public void GetMultipleDatesWithOtherSeparator()
        {
            var test = "2020-01-10;2020-01-12>2020-01-14;2020-01-16";

            var dates = test.GetDates(";");

            Assert.IsTrue(dates.Count() == 5);
        }

        [Test]
        public void ShiftedDates()
        {
            var givens = new DateTime[] { new DateTime(2020, 1, 10), new DateTime(2020, 1, 16) };

            var backs = givens.GetShifted(-1);

            Assert.True(backs.First() == new DateTime(2020, 1, 9));
            Assert.True(backs.Last() == new DateTime(2020, 1, 15));

            var ons = givens.GetShifted(1);

            Assert.True(ons.First() == new DateTime(2020, 1, 11));
            Assert.True(ons.Last() == new DateTime(2020, 1, 17));
        }

        #endregion Public Methods
    }
}
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
        public void BitmaskSingleBit()
        {
            var singleNoneBit = "0";

            var startOnlyNone = singleNoneBit.GetDates(DateTime.Now).ToArray();

            Assert.IsFalse(startOnlyNone.Any());

            var startAndEmptyEndNone = singleNoneBit.GetDates(DateTime.Now, default).ToArray();

            Assert.IsFalse(startAndEmptyEndNone.Any());

            var singleBit = "1";

            var startOnly = singleBit.GetDates(DateTime.Now);

            Assert.IsTrue(startOnly.Single() == DateTime.Today);

            var startAndEmptyEnd = singleBit.GetDates(DateTime.Now, default).ToArray();

            Assert.IsTrue(startAndEmptyEnd.Single() == DateTime.Today);
        }

        [Test]
        public void BitmaskWithEnd()
        {
            var bits = "1000000";

            var dates = bits.GetDates(DateTime.Now, DateTime.Now.AddDays(13)).ToArray();

            Assert.IsTrue(dates.Count() == 2);
        }

        [Test]
        public void BitmaskWithoutEnd()
        {
            var bits7 = "1111111";

            var dates7 = bits7.GetDates(DateTime.Now, default).ToArray();

            Assert.IsTrue(dates7.Count() == 7);

            var bits1 = "0001000";

            var dates1 = bits1.GetDates(DateTime.Now, default).ToArray();

            Assert.IsTrue(dates1.Count() == 1);
        }

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

        [Test]
        public void GetMultipleDatesFromEmpty()
        {
            var test = "  ";

            var dates = test.GetDates();

            Assert.IsFalse(dates.Any());
        }

        [Test]
        public void GetMultipleDatesFromfalse()
        {
            var test = "2020-02-31,2020-02-32";

            var dates = test.GetDates();

            Assert.IsFalse(dates.Any());
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
        public void GetNext()
        {
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Monday).DayOfWeek == DayOfWeek.Monday);
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Monday) >= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Tuesday).DayOfWeek == DayOfWeek.Tuesday);
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Tuesday) >= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Wednesday).DayOfWeek == DayOfWeek.Wednesday);
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Wednesday) >= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Thursday).DayOfWeek == DayOfWeek.Thursday);
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Thursday) >= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Friday).DayOfWeek == DayOfWeek.Friday);
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Friday) >= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Saturday).DayOfWeek == DayOfWeek.Saturday);
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Saturday) >= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Sunday).DayOfWeek == DayOfWeek.Sunday);
            Assert.IsTrue(DateTime.Today.GetNext(DayOfWeek.Sunday) >= DateTime.Today);
        }

        [Test]
        public void GetPrevious()
        {
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Monday).DayOfWeek == DayOfWeek.Monday);
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Monday) <= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Tuesday).DayOfWeek == DayOfWeek.Tuesday);
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Tuesday) <= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Wednesday).DayOfWeek == DayOfWeek.Wednesday);
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Wednesday) <= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Thursday).DayOfWeek == DayOfWeek.Thursday);
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Thursday) <= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Friday).DayOfWeek == DayOfWeek.Friday);
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Friday) <= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Saturday).DayOfWeek == DayOfWeek.Saturday);
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Saturday) <= DateTime.Today);

            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Sunday).DayOfWeek == DayOfWeek.Sunday);
            Assert.IsTrue(DateTime.Today.GetPrevious(DayOfWeek.Sunday) <= DateTime.Today);
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
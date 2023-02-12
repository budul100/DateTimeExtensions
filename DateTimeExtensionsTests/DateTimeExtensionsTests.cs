using DateTimeExtensions;
using NUnit.Framework;
using System;
using System.Linq;

namespace DateTimeExtensionsTests
{
    public class DateTimeExtensionsTests
    {
        #region Public Methods

        [Test]
        public void BitmaskSingleBit()
        {
            const string singleNoneBit = "F";
            const string singleBit = "T";

            var startOnlyNone = singleNoneBit.GetDates(DateTime.Now).ToArray();

            Assert.IsFalse(startOnlyNone.Length > 0);

            var startAndEmptyEndNone = singleNoneBit.GetDates(
                startDate: DateTime.Now,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.IsFalse(startAndEmptyEndNone.Length > 0);

            var startOnly = singleBit.GetDates(
                startDate: DateTime.Now,
                positiveBit: singleBit[0]).ToArray();

            Assert.IsTrue(startOnly.Single() == DateTime.Today);

            var startAndEmptyEnd = singleBit.GetDates(
                startDate: DateTime.Now,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.IsTrue(startAndEmptyEnd.Single() == DateTime.Today);
        }

        [Test]
        public void BitmaskWithEnd()
        {
            const string bits = "1000000";

            var dates = bits.GetDates(DateTime.Now, DateTime.Now.AddDays(13)).ToArray();

            Assert.IsTrue(dates.Length == 2);
        }

        [Test]
        public void BitmaskWithoutEnd()
        {
            const string bits7 = "1111111";

            var dates7 = bits7.GetDates(DateTime.Now, default).ToArray();

            Assert.IsTrue(dates7.Length == 7);

            const string bits1 = "0001000";

            var dates1 = bits1.GetDates(DateTime.Now, default).ToArray();

            Assert.IsTrue(dates1.Length == 1);
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
        public void GetDatesWithDaysOfWeek()
        {
            var daysOfWeek = new DayOfWeek[] { DayOfWeek.Friday, DayOfWeek.Tuesday };

            var from = DateTime.Now.AddDays(-6);
            var to = DateTime.Now.AddDays(7);

            var result = from.GetDates(
                to: to,
                daysOfWeek: daysOfWeek).ToArray();

            Assert.True(result.Length == 4);
        }

        [Test]
        public void GetDatesWithoutDaysOfWeek()
        {
            var daysOfWeek = Array.Empty<DayOfWeek>();

            var from = DateTime.Now.AddDays(-5);
            var to = DateTime.Now.AddDays(5);

            var result = from.GetDates(to, daysOfWeek).ToArray();

            Assert.True(result.Length == 11);
        }

        [Test]
        public void GetMultipleDatesFromEmpty()
        {
            const string test = "  ";

            var dates = test.GetDates();

            Assert.IsFalse(dates.Any());
        }

        [Test]
        public void GetMultipleDatesFromfalse()
        {
            const string test = "2020-02-31,2020-02-32";

            var dates = test.GetDates();

            Assert.IsFalse(dates.Any());
        }

        [Test]
        public void GetMultipleDatesFromString()
        {
            const string test = "2020-01-10,2020-01-12>2020-01-14,2020-01-16";

            var dates = test.GetDates();

            Assert.IsTrue(dates.Count() == 5);
        }

        [Test]
        public void GetMultipleDatesFromStringWithWrongOrder()
        {
            const string test = "2020-01-10,2020-01-14>2020-01-12,2020-01-16";

            Assert.Throws<FormatException>(() => test.GetDates());
        }

        [Test]
        public void GetMultipleDatesWithFromToSeparator()
        {
            const string test = "2020-01-10;2020-01-12>2020-01-14;2020-01-16";

            Assert.Throws<ArgumentException>(() => test.GetDates(">"));
        }

        [Test]
        public void GetMultipleDatesWithOtherSeparator()
        {
            const string test = "2020-01-10;2020-01-12>2020-01-14;2020-01-16";

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
using System;
using System.Linq;
using DateTimeExtensions;
using NUnit.Framework;

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

            Assert.That(startOnlyNone.Length == 0);

            var startAndEmptyEndNone = singleNoneBit.GetDates(
                startDate: DateTime.Now,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.That(startAndEmptyEndNone.Length == 0);

            var startOnly = singleBit.GetDates(
                startDate: DateTime.Now,
                positiveBit: singleBit[0]).ToArray();

            Assert.That(startOnly.Single() == DateTime.Today);

            var startAndEmptyEnd = singleBit.GetDates(
                startDate: DateTime.Now,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.That(startAndEmptyEnd.Single() == DateTime.Today);
        }

        [Test]
        public void BitmaskWithEnd()
        {
            const string bits = "1000000";

            var dates = bits.GetDates(DateTime.Now, DateTime.Now.AddDays(13)).ToArray();

            Assert.That(dates.Length == 2);
        }

        [Test]
        public void BitmaskWithoutEnd()
        {
            const string bits7 = "1111111";

            var dates7 = bits7.GetDates(DateTime.Now, default).ToArray();

            Assert.That(dates7.Length == 7);

            const string bits1 = "0001000";

            var dates1 = bits1.GetDates(DateTime.Now, default).ToArray();

            Assert.That(dates1.Length == 1);
        }

        [Test]
        public void CycleDatesMultiDay()
        {
            var cycle = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var inCycle = new DateTime(2020, 1, 12).GetCyclic(cycle);
            Assert.That(inCycle == new DateTime(2020, 1, 12));

            var atBorder = new DateTime(2020, 1, 10).GetCyclic(cycle);
            Assert.That(atBorder == new DateTime(2020, 1, 10));

            var leftAtBorder = new DateTime(2020, 1, 2).GetCyclic(cycle);
            Assert.That(leftAtBorder == new DateTime(2020, 1, 16));

            var leftToCycle = new DateTime(2020, 1, 3).GetCyclic(cycle);
            Assert.That(leftToCycle == new DateTime(2020, 1, 10));

            var rightToCycle = new DateTime(2020, 1, 30).GetCyclic(cycle);
            Assert.That(rightToCycle == new DateTime(2020, 1, 16));
        }

        [Test]
        public void CycleDatesSingleDay()
        {
            var cycle = new DateTime[] { new(2020, 1, 10) };

            var outCycle = new DateTime(2020, 1, 12).GetCyclic(cycle);
            Assert.That(outCycle == new DateTime(2020, 1, 10));

            var inCycle = new DateTime(2020, 1, 10).GetCyclic(cycle);
            Assert.That(inCycle == new DateTime(2020, 1, 10));
        }

        [Test]
        public void GetDatesFromDatesString()
        {
            const string test = "2020-01-10,2020-01-12>2020-01-14,2020-01-16";

            var dates = test.GetDates();

            Assert.That(dates.Count() == 5);
        }

        [Test]
        public void GetDatesFromEmptyString()
        {
            const string test = "  ";

            var dates = test.GetDates();

            Assert.That(!dates.Any());
        }

        [Test]
        public void GetDatesFromPeriodsString()
        {
            const string test = "2020-01-10,2020-01-12 10:00>2020-01-14 18:00,2020-01-16";

            var dates = test.GetDates();

            Assert.That(dates.Count() == 5);
        }

        [Test]
        public void GetDatesFromWrongFormat()
        {
            const string test = "2020-02-31,2020-02-32";

            var dates = test.GetDates();

            Assert.That(!dates.Any());
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

            Assert.That(result.Length == 4);
        }

        [Test]
        public void GetDatesWithoutDaysOfWeek()
        {
            var daysOfWeek = Array.Empty<DayOfWeek>();

            var from = DateTime.Now.AddDays(-5);
            var to = DateTime.Now.AddDays(5);

            var result = from.GetDates(to, daysOfWeek).ToArray();

            Assert.That(result.Length == 11);
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

            Assert.That(dates.Count() == 5);
        }

        [Test]
        public void GetNext()
        {
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Monday).DayOfWeek == DayOfWeek.Monday);
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Monday) >= DateTime.Today);

            Assert.That(DateTime.Today.GetNext(DayOfWeek.Tuesday).DayOfWeek == DayOfWeek.Tuesday);
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Tuesday) >= DateTime.Today);

            Assert.That(DateTime.Today.GetNext(DayOfWeek.Wednesday).DayOfWeek == DayOfWeek.Wednesday);
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Wednesday) >= DateTime.Today);

            Assert.That(DateTime.Today.GetNext(DayOfWeek.Thursday).DayOfWeek == DayOfWeek.Thursday);
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Thursday) >= DateTime.Today);

            Assert.That(DateTime.Today.GetNext(DayOfWeek.Friday).DayOfWeek == DayOfWeek.Friday);
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Friday) >= DateTime.Today);

            Assert.That(DateTime.Today.GetNext(DayOfWeek.Saturday).DayOfWeek == DayOfWeek.Saturday);
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Saturday) >= DateTime.Today);

            Assert.That(DateTime.Today.GetNext(DayOfWeek.Sunday).DayOfWeek == DayOfWeek.Sunday);
            Assert.That(DateTime.Today.GetNext(DayOfWeek.Sunday) >= DateTime.Today);
        }

        [Test]
        public void GetPeriods()
        {
            const string test = "2020-01-10,2020-01-12 10:00>2020-01-14 18:00,2020-01-16";

            var ranges = test.GetPeriods();

            Assert.That(ranges.Count() == 3);

            Assert.That(ranges.ElementAt(0).Item1.Day == 10);
            Assert.That(ranges.ElementAt(0).Item1.Hour == 0);
            Assert.That(ranges.ElementAt(0).Item2.Day == 10);
            Assert.That(ranges.ElementAt(0).Item2.Hour == 23);

            Assert.That(ranges.ElementAt(1).Item1.Day == 12);
            Assert.That(ranges.ElementAt(1).Item1.Hour == 10);
            Assert.That(ranges.ElementAt(1).Item2.Day == 14);
            Assert.That(ranges.ElementAt(1).Item2.Hour == 18);

            Assert.That(ranges.ElementAt(2).Item1.Day == 16);
            Assert.That(ranges.ElementAt(2).Item1.Hour == 0);
            Assert.That(ranges.ElementAt(2).Item2.Day == 16);
            Assert.That(ranges.ElementAt(2).Item2.Hour == 23);
        }

        [Test]
        public void GetPrevious()
        {
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Monday).DayOfWeek == DayOfWeek.Monday);
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Monday) <= DateTime.Today);

            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Tuesday).DayOfWeek == DayOfWeek.Tuesday);
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Tuesday) <= DateTime.Today);

            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Wednesday).DayOfWeek == DayOfWeek.Wednesday);
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Wednesday) <= DateTime.Today);

            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Thursday).DayOfWeek == DayOfWeek.Thursday);
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Thursday) <= DateTime.Today);

            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Friday).DayOfWeek == DayOfWeek.Friday);
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Friday) <= DateTime.Today);

            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Saturday).DayOfWeek == DayOfWeek.Saturday);
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Saturday) <= DateTime.Today);

            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Sunday).DayOfWeek == DayOfWeek.Sunday);
            Assert.That(DateTime.Today.GetPrevious(DayOfWeek.Sunday) <= DateTime.Today);
        }

        [Test]
        public void ShiftedDates()
        {
            var givens = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var backs = givens.GetShifted(-1);

            Assert.That(backs.First() == new DateTime(2020, 1, 9));
            Assert.That(backs.Last() == new DateTime(2020, 1, 15));

            var ons = givens.GetShifted(1);

            Assert.That(ons.First() == new DateTime(2020, 1, 11));
            Assert.That(ons.Last() == new DateTime(2020, 1, 17));
        }

        #endregion Public Methods
    }
}
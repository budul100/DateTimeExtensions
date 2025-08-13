using System;
using System.Linq;
using DateTimeExtensions;
using Xunit;

namespace DateTimeExtensionsTests
{
    public class DateTimeExtensionsTests
    {
        #region Public Methods

        [Fact]
        public void BitmaskSingleBit()
        {
            const string singleNoneBit = "F";
            const string singleBit = "T";

            var startOnlyNone = singleNoneBit.GetDates(DateTime.Now).ToArray();

            Assert.Empty(startOnlyNone);

            var startAndEmptyEndNone = singleNoneBit.GetDates(
                startDate: DateTime.Now,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.Empty(startAndEmptyEndNone);

            var startOnly = singleBit.GetDates(
                startDate: DateTime.Now,
                positiveBit: singleBit[0]).ToArray();

            Assert.True(startOnly.Single() == DateTime.Today);

            var startAndEmptyEnd = singleBit.GetDates(
                startDate: DateTime.Now,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.True(startAndEmptyEnd.Single() == DateTime.Today);
        }

        [Fact]
        public void BitmaskWithEnd()
        {
            const string bits = "1000000";

            var dates = bits.GetDates(DateTime.Now, DateTime.Now.AddDays(13)).ToArray();

            Assert.Equal(2, dates.Length);
        }

        [Fact]
        public void BitmaskWithoutEnd()
        {
            const string bits7 = "1111111";

            var dates7 = bits7.GetDates(DateTime.Now, default).ToArray();

            Assert.Equal(7, dates7.Length);

            const string bits1 = "0001000";

            var dates1 = bits1.GetDates(DateTime.Now, default).ToArray();

            Assert.Single(dates1);
        }

        [Fact]
        public void CycleDatesMultiDay()
        {
            var cycle = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var inCycle = new DateTime(2020, 1, 12).ShiftIntoCycle(cycle);
            Assert.True(inCycle == new DateTime(2020, 1, 12));

            var atBorder = new DateTime(2020, 1, 10).ShiftIntoCycle(cycle);
            Assert.True(atBorder == new DateTime(2020, 1, 10));

            var leftAtBorder = new DateTime(2020, 1, 2).ShiftIntoCycle(cycle);
            Assert.True(leftAtBorder == new DateTime(2020, 1, 16));

            var leftToCycle = new DateTime(2020, 1, 3).ShiftIntoCycle(cycle);
            Assert.True(leftToCycle == new DateTime(2020, 1, 10));

            var rightToCycle = new DateTime(2020, 1, 30).ShiftIntoCycle(cycle);
            Assert.True(rightToCycle == new DateTime(2020, 1, 16));
        }

        [Fact]
        public void CycleDatesSingleDay()
        {
            var cycle = new DateTime[] { new(2020, 1, 10) };

            var outCycle = new DateTime(2020, 1, 12).ShiftIntoCycle(cycle);
            Assert.True(outCycle == new DateTime(2020, 1, 10));

            var inCycle = new DateTime(2020, 1, 10).ShiftIntoCycle(cycle);
            Assert.True(inCycle == new DateTime(2020, 1, 10));
        }

        [Fact]
        public void GetDatesFromDatesString()
        {
            const string test = "2020-01-10,2020-01-12>2020-01-14,2020-01-16";

            var dates = test.GetDates();

            Assert.Equal(5, dates.Count());
        }

        [Fact]
        public void GetDatesFromEmptyString()
        {
            const string test = "  ";

            var dates = test.GetDates();

            Assert.False(dates.Any());
        }

        [Fact]
        public void GetDatesFromPeriodsString()
        {
            const string test = "2020-01-10,xxx,2020-01-12 18:00>2020-01-14 10:00,,2020-01-16";

            var dates = test.GetDates();

            Assert.Equal(5, dates.Count());
        }

        [Fact]
        public void GetDatesFromWrongFormat()
        {
            const string test = "2020-02-31,2020-02-32";

            var dates = test.GetDates();

            Assert.False(dates.Any());
        }

        [Fact]
        public void GetDatesWithDaysOfWeek()
        {
            var daysOfWeek = new DayOfWeek[] { DayOfWeek.Friday, DayOfWeek.Tuesday };

            var from = DateTime.Now.AddDays(-6);
            var to = DateTime.Now.AddDays(7);

            var result = from.GetDates(
                to: to,
                daysOfWeek: daysOfWeek).ToArray();

            Assert.Equal(4, result.Length);
        }

        [Fact]
        public void GetDatesWithoutDaysOfWeek()
        {
            var daysOfWeek = Array.Empty<DayOfWeek>();

            var from = DateTime.Now.AddDays(-5);
            var to = DateTime.Now.AddDays(5);

            var result = from.GetDates(to, daysOfWeek).ToArray();

            Assert.Equal(11, result.Length);
        }

        [Fact]
        public void GetMultipleDatesFromStringWithWrongOrder()
        {
            const string test = "2020-01-10,2020-01-14>2020-01-12,2020-01-16";

            Assert.Throws<FormatException>(() => test.GetDates());
        }

        [Fact]
        public void GetMultipleDatesWithFromToSeparator()
        {
            const string test = "2020-01-10;2020-01-12>2020-01-14;2020-01-16";

            Assert.Throws<ArgumentException>(() => test.GetDates(">"));
        }

        [Fact]
        public void GetMultipleDatesWithOtherSeparator()
        {
            const string test = "2020-01-10;2020-01-12>2020-01-14;2020-01-16";

            var dates = test.GetDates(";");

            Assert.Equal(5, dates.Count());
        }

        [Fact]
        public void GetNext()
        {
            Assert.Equal(DayOfWeek.Monday, DateTime.Today.GetNext(DayOfWeek.Monday).DayOfWeek);
            Assert.True(DateTime.Today.GetNext(DayOfWeek.Monday) >= DateTime.Today);

            Assert.Equal(DayOfWeek.Tuesday, DateTime.Today.GetNext(DayOfWeek.Tuesday).DayOfWeek);
            Assert.True(DateTime.Today.GetNext(DayOfWeek.Tuesday) >= DateTime.Today);

            Assert.Equal(DayOfWeek.Wednesday, DateTime.Today.GetNext(DayOfWeek.Wednesday).DayOfWeek);
            Assert.True(DateTime.Today.GetNext(DayOfWeek.Wednesday) >= DateTime.Today);

            Assert.Equal(DayOfWeek.Thursday, DateTime.Today.GetNext(DayOfWeek.Thursday).DayOfWeek);
            Assert.True(DateTime.Today.GetNext(DayOfWeek.Thursday) >= DateTime.Today);

            Assert.Equal(DayOfWeek.Friday, DateTime.Today.GetNext(DayOfWeek.Friday).DayOfWeek);
            Assert.True(DateTime.Today.GetNext(DayOfWeek.Friday) >= DateTime.Today);

            Assert.Equal(DayOfWeek.Saturday, DateTime.Today.GetNext(DayOfWeek.Saturday).DayOfWeek);
            Assert.True(DateTime.Today.GetNext(DayOfWeek.Saturday) >= DateTime.Today);

            Assert.Equal(DayOfWeek.Sunday, DateTime.Today.GetNext(DayOfWeek.Sunday).DayOfWeek);
            Assert.True(DateTime.Today.GetNext(DayOfWeek.Sunday) >= DateTime.Today);
        }

        [Fact]
        public void GetPeriods()
        {
            const string test = "2020-01-10,xxx,2020-01-12 18:00>2020-01-14 10:00,,2020-01-16";

            var ranges = test.GetPeriods();

            Assert.Equal(3, ranges.Count());

            Assert.Equal(10, ranges.ElementAt(0).Item1.Day);
            Assert.Equal(0, ranges.ElementAt(0).Item1.Hour);
            Assert.Equal(10, ranges.ElementAt(0).Item2.Day);
            Assert.Equal(23, ranges.ElementAt(0).Item2.Hour);

            Assert.Equal(12, ranges.ElementAt(1).Item1.Day);
            Assert.Equal(18, ranges.ElementAt(1).Item1.Hour);
            Assert.Equal(14, ranges.ElementAt(1).Item2.Day);
            Assert.Equal(10, ranges.ElementAt(1).Item2.Hour);

            Assert.Equal(16, ranges.ElementAt(2).Item1.Day);
            Assert.Equal(0, ranges.ElementAt(2).Item1.Hour);
            Assert.Equal(16, ranges.ElementAt(2).Item2.Day);
            Assert.Equal(23, ranges.ElementAt(2).Item2.Hour);
        }

        [Fact]
        public void GetPrevious()
        {
            Assert.Equal(DayOfWeek.Monday, DateTime.Today.GetPrevious(DayOfWeek.Monday).DayOfWeek);
            Assert.True(DateTime.Today.GetPrevious(DayOfWeek.Monday) <= DateTime.Today);

            Assert.Equal(DayOfWeek.Tuesday, DateTime.Today.GetPrevious(DayOfWeek.Tuesday).DayOfWeek);
            Assert.True(DateTime.Today.GetPrevious(DayOfWeek.Tuesday) <= DateTime.Today);

            Assert.Equal(DayOfWeek.Wednesday, DateTime.Today.GetPrevious(DayOfWeek.Wednesday).DayOfWeek);
            Assert.True(DateTime.Today.GetPrevious(DayOfWeek.Wednesday) <= DateTime.Today);

            Assert.Equal(DayOfWeek.Thursday, DateTime.Today.GetPrevious(DayOfWeek.Thursday).DayOfWeek);
            Assert.True(DateTime.Today.GetPrevious(DayOfWeek.Thursday) <= DateTime.Today);

            Assert.Equal(DayOfWeek.Friday, DateTime.Today.GetPrevious(DayOfWeek.Friday).DayOfWeek);
            Assert.True(DateTime.Today.GetPrevious(DayOfWeek.Friday) <= DateTime.Today);

            Assert.Equal(DayOfWeek.Saturday, DateTime.Today.GetPrevious(DayOfWeek.Saturday).DayOfWeek);
            Assert.True(DateTime.Today.GetPrevious(DayOfWeek.Saturday) <= DateTime.Today);

            Assert.Equal(DayOfWeek.Sunday, DateTime.Today.GetPrevious(DayOfWeek.Sunday).DayOfWeek);
            Assert.True(DateTime.Today.GetPrevious(DayOfWeek.Sunday) <= DateTime.Today);
        }

        [Fact]
        public void ShiftedDates()
        {
            var givens = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var backs = givens.Shift(-1);

            Assert.True(backs.First() == new DateTime(2020, 1, 9));
            Assert.True(backs.Last() == new DateTime(2020, 1, 15));

            var ons = givens.Shift(1);

            Assert.True(ons.First() == new DateTime(2020, 1, 11));
            Assert.True(ons.Last() == new DateTime(2020, 1, 17));
        }

        #endregion Public Methods
    }
}
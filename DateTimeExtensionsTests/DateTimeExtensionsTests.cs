using System;
using System.Globalization;
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

            // Fixed start with a time of day, the result is date-only
            var start = new DateTime(2020, 1, 10, 15, 30, 0);

            var startOnlyNone = singleNoneBit.GetDates(start).ToArray();

            Assert.Empty(startOnlyNone);

            var startAndEmptyEndNone = singleNoneBit.GetDates(
                startDate: start,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.Empty(startAndEmptyEndNone);

            var startOnly = singleBit.GetDates(
                startDate: start,
                positiveBit: singleBit[0]).ToArray();

            Assert.Equal(start.Date, startOnly.Single());

            var startAndEmptyEnd = singleBit.GetDates(
                startDate: start,
                endDate: default,
                positiveBit: singleBit[0]).ToArray();

            Assert.Equal(start.Date, startAndEmptyEnd.Single());
        }

        [Fact]
        public void BitmaskWithEnd()
        {
            const string bits = "1000000";

            var start = new DateTime(2020, 1, 6);

            var dates = bits.GetDates(start, start.AddDays(13)).ToArray();

            Assert.Equal(2, dates.Length);
        }

        [Fact]
        public void BitmaskWithoutEnd()
        {
            var start = new DateTime(2020, 1, 6);

            const string bits7 = "1111111";

            var dates7 = bits7.GetDates(start, default).ToArray();

            Assert.Equal(7, dates7.Length);

            const string bits1 = "0001000";

            var dates1 = bits1.GetDates(start, default).ToArray();

            Assert.Single(dates1);
        }

        [Fact]
        public void BitmaskWithTimeOfDay()
        {
            // Start has a time component, end is a pure date on the last relevant day
            var start = new DateTime(2020, 1, 10, 10, 0, 0);
            var end = new DateTime(2020, 1, 17);

            var dates = "1000000".GetDates(start, end).ToArray();

            Assert.Equal(new[] { new DateTime(2020, 1, 10), new DateTime(2020, 1, 17) }, dates);
        }

        [Fact]
        public void CycleDatesMultiDay()
        {
            var cycle = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var inPeriod = new DateTime(2020, 1, 12).MoveInPeriod(cycle);
            Assert.True(inPeriod == new DateTime(2020, 1, 12));

            var atBorderRight = new DateTime(2020, 1, 17).MoveInPeriod(cycle);
            Assert.True(atBorderRight == new DateTime(2020, 1, 10));

            var atBorderLeft = new DateTime(2020, 1, 9).MoveInPeriod(cycle);
            Assert.True(atBorderLeft == new DateTime(2020, 1, 16));

            var leftToPeriod = new DateTime(2020, 1, 2).MoveInPeriod(cycle, false);
            Assert.True(leftToPeriod == new DateTime(2020, 1, 9));

            var leftToCycle = new DateTime(2020, 1, 2).MoveInPeriod(cycle, true);
            Assert.True(leftToCycle == new DateTime(2020, 1, 16));

            var rightToPeriod = new DateTime(2020, 1, 24).MoveInPeriod(cycle, false);
            Assert.True(rightToPeriod == new DateTime(2020, 1, 17));

            var rightToCycle = new DateTime(2020, 1, 24).MoveInPeriod(cycle, true);
            Assert.True(rightToCycle == new DateTime(2020, 1, 10));
        }

        [Fact]
        public void CycleDatesSingleDay()
        {
            var cycle = new DateTime[] { new(2020, 1, 10) };

            var inPeriod = new DateTime(2020, 1, 10).MoveInPeriod(cycle);
            Assert.True(inPeriod == new DateTime(2020, 1, 10));

            var outPeriod = new DateTime(2020, 1, 12).MoveInPeriod(cycle);
            Assert.True(outPeriod == new DateTime(2020, 1, 10));
        }

        [Fact]
        public void CycleDatesKeepKindOfInput()
        {
            var cycle = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var utc = new DateTime(2020, 1, 24, 10, 0, 0, DateTimeKind.Utc);

            var moved = utc.MoveInPeriod(cycle);

            Assert.Equal(new DateTime(2020, 1, 17, 10, 0, 0), moved);
            Assert.Equal(DateTimeKind.Utc, moved.Kind);
        }

        [Fact]
        public void CycleDatesOfSequence()
        {
            var cycle = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var dates = new DateTime[] { new(2020, 1, 2), new(2020, 1, 12), new(2020, 1, 24) };

            var moved = dates.MoveInPeriod(cycle, true).ToArray();

            Assert.Equal(new DateTime[] { new(2020, 1, 16), new(2020, 1, 12), new(2020, 1, 10) }, moved);
        }

        [Fact]
        public void CycleDatesOfSequenceWithoutPeriod()
        {
            var dates = new DateTime[] { new(2020, 1, 2), new(2020, 1, 12) };

            Assert.Equal(dates, dates.MoveInPeriod(period: null).ToArray());
            Assert.Equal(dates, dates.MoveInPeriod(Array.Empty<DateTime>()).ToArray());
        }

        [Theory]
        [InlineData("2020-01-16 12:00", false, "2020-01-16 12:00")] // inside last day of period
        [InlineData("2020-01-10 08:00", false, "2020-01-10 08:00")] // inside first day of period
        [InlineData("2020-01-09 23:00", false, "2020-01-16 23:00")] // one hour before period
        [InlineData("2020-01-17 01:00", false, "2020-01-10 01:00")] // one hour after period
        [InlineData("2020-01-02 12:00", true, "2020-01-16 12:00")]
        public void CycleDatesWithTimeOfDay(string input, bool isCyclic, string expected)
        {
            var cycle = new DateTime[] { new(2020, 1, 10), new(2020, 1, 16) };

            var actual = Parse(input).MoveInPeriod(cycle, isCyclic);

            Assert.Equal(Parse(expected), actual);
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
        public void GetDatesFromOverlappingSections()
        {
            const string test = "2020-01-10,2020-01-10,2020-01-09>2020-01-10";

            var dates = test.GetDates().ToArray();

            Assert.Equal(new DateTime[] { new(2020, 1, 9), new(2020, 1, 10) }, dates);
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

            var from = new DateTime(2020, 1, 1);
            var to = new DateTime(2020, 1, 14);

            var result = from.GetDates(
                to: to,
                daysOfWeek: daysOfWeek).ToArray();

            Assert.Equal(
                new DateTime[] { new(2020, 1, 3), new(2020, 1, 7), new(2020, 1, 10), new(2020, 1, 14) },
                result);
        }

        [Fact]
        public void GetDatesWithoutDaysOfWeek()
        {
            var daysOfWeek = Array.Empty<DayOfWeek>();

            var from = new DateTime(2020, 1, 1);
            var to = new DateTime(2020, 1, 11);

            var result = from.GetDates(to, daysOfWeek).ToArray();

            Assert.Equal(11, result.Length);
        }

        [Fact]
        public void GetDatesWithReversedBorders()
        {
            var result = new DateTime(2020, 1, 12).GetDates(new DateTime(2020, 1, 10)).ToArray();

            Assert.Equal(new DateTime[] { new(2020, 1, 10), new(2020, 1, 11), new(2020, 1, 12) }, result);
        }

        [Fact]
        public void GetDatesWithTimeOfDay()
        {
            // The last day must not get lost because its time of day is earlier than the start's
            var from = new DateTime(2020, 1, 10, 10, 0, 0);
            var to = new DateTime(2020, 1, 12, 8, 0, 0);

            var result = from.GetDates(to).ToArray();

            Assert.Equal(new DateTime[] { new(2020, 1, 10), new(2020, 1, 11), new(2020, 1, 12) }, result);
        }

        [Fact]
        public void GetDatesUpToMaxValueDoesNotOverflow()
        {
            var to = DateTime.MaxValue;
            var from = to.Date.AddDays(-1);

            var result = from.GetDates(to).ToArray();

            Assert.Equal(2, result.Length);
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

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        public void GetDatesWithMissingSeparator(string separator)
        {
            // Otherwise string.Split would fall back to whitespace and break values with a time of day
            const string test = "2020-01-10";

            Assert.ThrowsAny<ArgumentException>(() => test.GetDates(separator));
            Assert.ThrowsAny<ArgumentException>(() => test.GetPeriods(separator));
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

        // 2020-01-01 is a Wednesday
        [Theory]
        [InlineData(DayOfWeek.Wednesday, "2020-01-01")]
        [InlineData(DayOfWeek.Friday, "2020-01-03")]
        [InlineData(DayOfWeek.Tuesday, "2020-01-07")]
        public void GetNextFromFixedDate(DayOfWeek day, string expected)
        {
            Assert.Equal(Parse(expected), new DateTime(2020, 1, 1).GetNext(day));
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
        public void GetPeriodsWithFromToSeparator()
        {
            const string test = "2020-01-10;2020-01-12>2020-01-14";

            Assert.Throws<ArgumentException>(() => test.GetPeriods(">"));
        }

        [Fact]
        public void GetPeriodsWithSingleValueWithTimeOfDay()
        {
            // A single value covers the rest of its own day and must not spill into the next day
            var period = "2020-01-12 18:00".GetPeriods().Single();

            Assert.Equal(new DateTime(2020, 1, 12, 18, 0, 0), period.From);
            Assert.Equal(new DateTime(2020, 1, 13).AddTicks(-1), period.To);
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

        // 2020-01-01 is a Wednesday
        [Theory]
        [InlineData(DayOfWeek.Wednesday, "2020-01-01")]
        [InlineData(DayOfWeek.Monday, "2019-12-30")]
        [InlineData(DayOfWeek.Thursday, "2019-12-26")]
        public void GetPreviousFromFixedDate(DayOfWeek day, string expected)
        {
            Assert.Equal(Parse(expected), new DateTime(2020, 1, 1).GetPrevious(day));
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

        #region Private Methods

        private static DateTime Parse(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture);

        #endregion Private Methods
    }
}

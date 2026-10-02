using System;
using System.Collections.Generic;
using System.Linq;

namespace DateTimeExtensions
{
    public static class DateTimeExtensions
    {
        #region Public Fields

        public const string FromToDatesSeparator = ">";

        #endregion Public Fields

        #region Private Fields

        private static readonly string[] fromToDatesSeparators = new string[] { FromToDatesSeparator };

        private static readonly TimeSpan oneDay = new TimeSpan(
            days: 1,
            hours: 0,
            minutes: 0,
            seconds: 0).Subtract(new TimeSpan(1));

        #endregion Private Fields

        #region Public Methods

        public static IEnumerable<DateTime> GetDates(this DateTime from, DateTime to,
            IEnumerable<DayOfWeek> daysOfWeek = default)
        {
            var start = from <= to ? from : to;
            var end = to >= from ? to : from;

            var filter = daysOfWeek != default
                ? new HashSet<DayOfWeek>(daysOfWeek)
                : default;

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (!(filter?.Count > 0) || filter.Contains(date.DayOfWeek))
                {
                    yield return date;
                }
            }
        }

        public static IEnumerable<DateTime> GetDates(this string bitMask, DateTime startDate,
            DateTime? endDate = default, char positiveBit = ValueExtensions.PositiveBit)
        {
            var bits = bitMask?.GetBits(
                positiveBit: positiveBit).ToArray();

            if (!(bits?.Length > 0))
            {
                yield break;
            }

            // Compare on date level only, the result is date-only anyway
            startDate = startDate.Date;

            endDate = endDate == default && startDate != default
                ? startDate.AddDays(bits.Max())
                : endDate?.Date;

            do
            {
                foreach (var bit in bits)
                {
                    var result = startDate.AddDays(bit);

                    if (result > endDate)
                    {
                        yield break;
                    }

                    yield return result;
                }

                startDate = startDate.AddDays(bitMask.Length);
            }
            while (startDate <= endDate);
        }

        public static IEnumerable<DateTime> GetDates(this string dates, string separator = ",")
        {
            ValidateSeparator(separator);

            var result = default(IEnumerable<DateTime>);

            if (!string.IsNullOrWhiteSpace(dates))
            {
                var sectionSeparators = new string[] { separator };

                var sections = dates.Split(
                    separator: sectionSeparators,
                    options: StringSplitOptions.RemoveEmptyEntries);

                result = sections.SelectDates()
                    .OrderBy(d => d).ToArray();
            }

            return result
                ?? Enumerable.Empty<DateTime>();
        }

        public static DateTime GetNext(this DateTime start, DayOfWeek day)
        {
            var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;

            return start.AddDays(daysToAdd);
        }

        public static IEnumerable<(DateTime From, DateTime To)> GetPeriods(this string dates, string separator = ",")
        {
            ValidateSeparator(separator);

            if (string.IsNullOrWhiteSpace(dates))
            {
                return Enumerable.Empty<(DateTime From, DateTime To)>();
            }

            var sections = dates.Split(
                separator: new[] { separator },
                options: StringSplitOptions.RemoveEmptyEntries);

            // ValueTuple compares lexicographically: From first, then To
            return sections.SelectPeriods()
                .OrderBy(p => p).ToArray();
        }

        public static DateTime GetPrevious(this DateTime start, DayOfWeek day)
        {
            var daysToAdd = ((int)day - (int)start.DayOfWeek - 7) % 7;

            return start.AddDays(daysToAdd);
        }

        public static DateTime? MoveInPeriod(this DateTime? date, IEnumerable<DateTime> period, bool isCyclic = false)
        {
            var result = default(DateTime?);

            if (date.HasValue)
            {
                result = date.Value.MoveInPeriod(
                    period: period,
                    isCyclic: isCyclic);
            }

            return result;
        }

        public static DateTime MoveInPeriod(this DateTime date, IEnumerable<DateTime> period, bool isCyclic = false)
        {
            var days = period?.Select(d => d.Date).ToArray();

            if (!(days?.Length > 0))
            {
                return date;
            }

            var from = days.Min();
            var to = days.Max();
            var day = date.Date;

            var duration = (to - from).Days + 1;

            DateTime result;

            if (duration < 2)
            {
                result = from;
            }
            else if (day < from)
            {
                // Non-cyclic shifts by exactly one period length, cyclic wraps into the period
                var distance = (from - day).Days - 1;

                result = isCyclic
                    ? to.AddDays(-(distance % duration))
                    : to.AddDays(-distance);
            }
            else if (day > to)
            {
                var distance = (day - to).Days - 1;

                result = isCyclic
                    ? from.AddDays(distance % duration)
                    : from.AddDays(distance);
            }
            else
            {
                result = day;
            }

            // Keep the time of day of the original value
            return result.Add(date.TimeOfDay);
        }

        public static IEnumerable<DateTime> MoveInPeriod(this IEnumerable<DateTime> dates,
            IEnumerable<DateTime> period, bool isCyclic = false)
        {
            if (dates == default)
            {
                yield break;
            }

            // Materialize once instead of per date
            var days = period?.ToArray();

            foreach (var date in dates)
            {
                yield return date.MoveInPeriod(
                    period: days,
                    isCyclic: isCyclic);
            }
        }

        public static IEnumerable<DateTime> Shift(this IEnumerable<DateTime> dates, int shift)
        {
            if (dates == default)
            {
                yield break;
            }

            foreach (var date in dates)
            {
                yield return date.Shift(shift);
            }
        }

        public static DateTime Shift(this DateTime value, int shift)
        {
            return value.AddDays(shift);
        }

        public static DateTime? Shift(this DateTime? value, int shift)
        {
            return value?.Shift(shift);
        }

        public static DateTime ToDateTime(this TimeSpan value)
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(value),
                    message: "Negative time spans cannot be converted to a DateTime.");
            }

            return new DateTime(value.Ticks);
        }

        public static DateTime? ToDateTime(this TimeSpan? value)
        {
            var result = default(DateTime?);

            if (value.HasValue)
            {
                result = value.Value.ToDateTime();
            }

            return result;
        }

        public static DateTime? ToDateTime(this string value)
        {
            var result = default(DateTime?);

            if (DateTime.TryParse(
                s: value,
                result: out DateTime parsed))
            {
                result = parsed;
            }

            return result;
        }

        public static DateTime ToUnspecified(this DateTime value)
        {
            var result = DateTime.SpecifyKind(
                value: value,
                kind: DateTimeKind.Unspecified);

            return result;
        }

        public static DateTime? ToUnspecified(this DateTime? value)
        {
            var result = default(DateTime?);

            if (value.HasValue)
            {
                result = value.Value.ToUnspecified();
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static IEnumerable<DateTime> SelectDates(this IEnumerable<string> sections)
        {
            foreach (var section in sections)
            {
                var currents = section.Split(
                    separator: fromToDatesSeparators,
                    options: StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.ToDateTime()?.Date)
                    .Where(d => d.HasValue).ToArray();

                if (currents?.Length == 1)
                {
                    yield return currents.Single().Value.Date;
                }
                else if (currents?.Length > 1)
                {
                    var from = currents[0].Value;
                    var to = currents.Last().Value;

                    if (to < from)
                    {
                        throw new FormatException(
                            message: "The dates order is wrong. The first date is later " +
                                $"than the second date: {section}.");
                    }

                    for (var date = from; date <= to; date = date.AddDays(1))
                    {
                        yield return date.Date;
                    }
                }
            }
        }

        private static IEnumerable<(DateTime, DateTime)> SelectPeriods(this IEnumerable<string> sections)
        {
            foreach (var section in sections)
            {
                var currents = section.Split(
                    separator: fromToDatesSeparators,
                    options: StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.ToDateTime())
                    .Where(d => d.HasValue).ToArray();

                if (currents?.Length > 0)
                {
                    DateTime from;
                    DateTime to;

                    if (currents.Length == 1)
                    {
                        from = currents.Single().Value;
                        to = from.Add(oneDay);
                    }
                    else
                    {
                        from = currents[0].Value;
                        to = currents.Last().Value.TimeOfDay == TimeSpan.Zero
                            ? currents.Last().Value.Add(oneDay)
                            : currents.Last().Value;

                        if (to < from)
                        {
                            throw new FormatException(
                                message: "The dates order is wrong. The first date is later " +
                                    $"than the second date: {section}.");
                        }
                    }

                    yield return (from, to);
                }
            }
        }

        private static void ValidateSeparator(string separator)
        {
            if (separator?.Contains(FromToDatesSeparator) ?? false)
            {
                throw new ArgumentException(
                    message: $"The argument splitter cannot be '{FromToDatesSeparator}' " +
                        "since it is used to split from and to values of periods.",
                    paramName: nameof(separator));
            }
        }

        #endregion Private Methods
    }
}
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

        #endregion Private Fields

        #region Public Methods

        public static IEnumerable<DateTime> GetCyclic(this IEnumerable<DateTime> dates, IEnumerable<DateTime> cycle)
        {
            if (dates?.Any() ?? false)
            {
                foreach (var date in dates)
                {
                    yield return date.GetCyclic(cycle);
                }
            }
        }

        public static DateTime? GetCyclic(this DateTime? date, IEnumerable<DateTime> cycle)
        {
            var result = default(DateTime?);

            if (date.HasValue)
            {
                result = date.Value.GetCyclic(cycle);
            }

            return result;
        }

        public static DateTime GetCyclic(this DateTime date, IEnumerable<DateTime> cycle)
        {
            var result = date;

            if (cycle?.Any() ?? false)
            {
                var from = cycle.Min();
                var to = cycle.Max();

                var duration = to.GetAbsDuration(from).Days + 1;

                if (duration < 2)
                {
                    result = from;
                }
                else if (date < from)
                {
                    var distance = from.GetAbsDuration(date).Days - 1;
                    result = to.AddDays((distance % duration) * -1);
                }
                else if (date > to)
                {
                    var distance = date.GetAbsDuration(to).Days - 1;
                    result = from.AddDays(distance % duration);
                }
                else
                {
                    result = date;
                }
            }

            return result;
        }

        public static IEnumerable<DateTime> GetDates(this DateTime from, DateTime to, IEnumerable<DayOfWeek> daysOfWeek = default)
        {
            var start = from <= to ? from : to;
            var end = to >= from ? to : from;

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (!(daysOfWeek?.Any() ?? false) || daysOfWeek.Contains(date.DayOfWeek))
                {
                    yield return date;
                }
            }
        }

        public static IEnumerable<DateTime> GetDates(this string bitMask, DateTime startDate, DateTime? endDate = default)
        {
            var bits = bitMask?
                .GetBits().ToArray();

            if (bits?.Any() ?? false)
            {
                if (endDate == default && startDate != default)
                {
                    endDate = startDate.AddDays(bits.Last());
                }

                do
                {
                    foreach (var bit in bits)
                    {
                        var result = startDate.AddDays(bit);

                        if (result > endDate)
                            yield break;

                        yield return result.Date;
                    }

                    startDate = startDate.AddDays(bitMask.Length);
                }
                while (startDate <= endDate);
            }
        }

        public static IEnumerable<DateTime> GetDates(this string dates, string separator = ",")
        {
            if (separator?.Contains(FromToDatesSeparator) ?? false)
            {
                throw new ArgumentException(
                    $"The argument splitter cannot be '{FromToDatesSeparator}' since it is used to split from and to values of periods.",
                    nameof(separator));
            }

            var result = default(IEnumerable<DateTime>);

            if (!string.IsNullOrWhiteSpace(dates))
            {
                var sectionSeparators = new string[] { separator };

                var sections = dates.Split(
                    separator: sectionSeparators,
                    options: StringSplitOptions.RemoveEmptyEntries);

                result = sections.GetDates()
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

        public static DateTime GetPrevious(this DateTime start, DayOfWeek day)
        {
            var daysToAdd = ((int)day - (int)start.DayOfWeek - 7) % 7;

            return start.AddDays(daysToAdd);
        }

        public static IEnumerable<DateTime> GetShifted(this IEnumerable<DateTime> dates, int shift)
        {
            if (dates?.Any() ?? false)
            {
                foreach (var date in dates)
                {
                    yield return date.GetShifted(shift);
                }
            }
        }

        public static DateTime GetShifted(this DateTime date, int shift)
        {
            return date.AddDays(shift);
        }

        public static DateTime? GetShifted(this DateTime? date, int shift)
        {
            return date?.GetShifted(shift);
        }

        public static DateTime ToDateTime(this TimeSpan time)
        {
            var result = new DateTime(time.Ticks);

            return result;
        }

        public static DateTime? ToDateTime(this TimeSpan? time)
        {
            var result = default(DateTime?);

            if (time.HasValue)
            {
                result = time.Value.ToDateTime();
            }

            return result;
        }

        public static DateTime? ToDateTime(this string date)
        {
            var result = default(DateTime?);

            if (DateTime.TryParse(date, out DateTime newDate))
            {
                result = newDate;
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static IEnumerable<DateTime> GetDates(this IEnumerable<string> sections)
        {
            foreach (var section in sections)
            {
                var currents = section.Split(
                    separator: fromToDatesSeparators,
                    options: StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.ToDateTime())
                    .Where(d => d.HasValue)
                    .OrderBy(d => d).ToArray();

                if (currents?.Any() ?? false)
                {
                    var from = currents[0].Value;
                    var to = currents.Last().Value;

                    for (var date = from; date <= to; date = date.AddDays(1))
                    {
                        yield return date.Date;
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
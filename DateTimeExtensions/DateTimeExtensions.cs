using System;
using System.Collections.Generic;
using System.Linq;

namespace DateTimeExtensions;

public static class DateTimeExtensions
{
    #region Public Fields

    public const string FromToDatesSeparator = ">";

    #endregion Public Fields

    #region Private Fields

    private static readonly string[] fromToDatesSeparators = [FromToDatesSeparator];

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
        // Compare on date level only, consistent with the bitmask overload
        var start = from <= to ? from.Date : to.Date;
        var end = from <= to ? to.Date : from.Date;

        var allowed = GetDayFlags(daysOfWeek);

        // Counting instead of comparing avoids an overflow when end is DateTime.MaxValue
        var count = (end - start).Days;

        for (var offset = 0; offset <= count; offset++)
        {
            var date = start.AddDays(offset);

            if (allowed == default || allowed[(int)date.DayOfWeek])
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

            // Overlapping sections must not produce duplicates
            result = sections.SelectDates()
                .Distinct().OrderBy(d => d).ToArray();
        }

        return result
            ?? [];
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
            return [];
        }

        var sections = dates.Split(
            separator: [separator],
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
        if (!TryGetPeriodBounds(
            period: period,
            periodStart: out var periodStart,
            periodEnd: out var periodEnd))
        {
            return date;
        }

        return MoveIntoBounds(
            date: date,
            periodStart: periodStart,
            periodEnd: periodEnd,
            isCyclic: isCyclic);
    }

    public static IEnumerable<DateTime> MoveInPeriod(this IEnumerable<DateTime> dates,
        IEnumerable<DateTime> period, bool isCyclic = false)
    {
        if (dates == default)
        {
            yield break;
        }

        // The bounds are determined once instead of per date
        var hasPeriod = TryGetPeriodBounds(
            period: period,
            periodStart: out var periodStart,
            periodEnd: out var periodEnd);

        foreach (var date in dates)
        {
            yield return hasPeriod
                ? MoveIntoBounds(
                    date: date,
                    periodStart: periodStart,
                    periodEnd: periodEnd,
                    isCyclic: isCyclic)
                : date;
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

    private static bool[] GetDayFlags(IEnumerable<DayOfWeek> daysOfWeek)
    {
        var result = default(bool[]);

        if (daysOfWeek != default)
        {
            var flags = new bool[7];
            var hasFlags = false;

            foreach (var day in daysOfWeek)
            {
                var index = (int)day;

                if (index >= 0 && index < flags.Length)
                {
                    flags[index] = true;
                    hasFlags = true;
                }
            }

            // No (valid) day means no filter
            if (hasFlags)
            {
                result = flags;
            }
        }

        return result;
    }

    private static DateTime MoveIntoBounds(DateTime date, DateTime periodStart, DateTime periodEnd, bool isCyclic)
    {
        var day = date.Date;

        var duration = (periodEnd - periodStart).Days + 1;

        DateTime result;

        if (duration < 2)
        {
            result = periodStart;
        }
        else if (day < periodStart)
        {
            // Non-cyclic shifts by exactly one period length, cyclic wraps into the period
            var distance = (periodStart - day).Days - 1;

            result = isCyclic
                ? periodEnd.AddDays(-(distance % duration))
                : periodEnd.AddDays(-distance);
        }
        else if (day > periodEnd)
        {
            var distance = (day - periodEnd).Days - 1;

            result = isCyclic
                ? periodStart.AddDays(distance % duration)
                : periodStart.AddDays(distance);
        }
        else
        {
            result = day;
        }

        // Keep the time of day and the kind of the original value
        return DateTime.SpecifyKind(
            value: result.Add(date.TimeOfDay),
            kind: date.Kind);
    }

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
                    // A single value covers the rest of its own day, also with a time of day
                    from = currents.Single().Value;
                    to = from.Date.Add(oneDay);
                }
                else
                {
                    from = currents[0].Value;

                    // An end value at 00:00 is treated as a pure date and covers the whole day
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

    private static bool TryGetPeriodBounds(IEnumerable<DateTime> period, out DateTime periodStart,
        out DateTime periodEnd)
    {
        periodStart = DateTime.MaxValue;
        periodEnd = DateTime.MinValue;

        var found = false;

        if (period != default)
        {
            foreach (var current in period)
            {
                var day = current.Date;

                if (day < periodStart)
                {
                    periodStart = day;
                }

                if (day > periodEnd)
                {
                    periodEnd = day;
                }

                found = true;
            }
        }

        return found;
    }

    private static void ValidateSeparator(string separator)
    {
        // A null or empty separator would make string.Split fall back to whitespace
        ArgumentException.ThrowIfNullOrEmpty(separator);

        if (separator.Contains(FromToDatesSeparator))
        {
            throw new ArgumentException(
                message: $"The argument splitter cannot be '{FromToDatesSeparator}' " +
                    "since it is used to split from and to values of periods.",
                paramName: nameof(separator));
        }
    }

    #endregion Private Methods
}

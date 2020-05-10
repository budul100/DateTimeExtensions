using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DateTimeExtensions
{
    public static class Extensions
    {
        #region Private Fields

        private const char NegativeBit = '0';
        private const char PositiveBit = '1';

        private static readonly Regex timespanPattern =
            new Regex(@"((?<d1>\d{1,2})\.)?(?<h>\d{1,2})\:(?<m>\d{1,2})(\:(?<s>\d{1,2}))?(\[\+(?<d2>\d)\])?.*");

        #endregion Private Fields

        #region Public Methods

        public static TimeSpan? AddDays(this TimeSpan? time, int days)
        {
            return time?.AddDays(days)
                ?? default(TimeSpan?);
        }

        public static TimeSpan AddDays(this TimeSpan time, int days)
        {
            var additionalDays = new TimeSpan(
                days: days,
                hours: 0,
                minutes: 0,
                seconds: 0);

            var result = time.Add(additionalDays);

            return result;
        }

        public static TimeSpan? GetAbsDuration(this TimeSpan? from, TimeSpan? to)
        {
            var result = default(TimeSpan?);

            if (from.HasValue && to.HasValue)
            {
                result = from.Value.GetAbsDuration(to.Value);
            }

            return result;
        }

        public static TimeSpan GetAbsDuration(this TimeSpan from, TimeSpan to)
        {
            var ticks = Math.Abs(to.Subtract(from).Ticks);
            var result = new TimeSpan(ticks);

            return result;
        }

        public static TimeSpan? GetAbsDuration(this DateTime? from, DateTime? to)
        {
            var result = default(TimeSpan?);

            if (from.HasValue && to.HasValue)
            {
                result = from.Value.GetAbsDuration(to.Value);
            }

            return result;
        }

        public static TimeSpan GetAbsDuration(this DateTime from, DateTime to)
        {
            var ticks = Math.Abs(to.Subtract(from).Ticks);
            var result = new TimeSpan(ticks);

            return result;
        }

        public static IEnumerable<int> GetBits(this string bitMask)
        {
            if (bitMask?.Any() ?? false)
            {
                var bits = bitMask.ToCharArray();

                for (int i = 0; i < bits.Length; i++)
                {
                    if (bits[i] == PositiveBit)
                    {
                        yield return i;
                    }
                }
            }
        }

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

        public static IEnumerable<DateTime> GetDates(this string bitMask, DateTime startDate, DateTime? endDate = null)
        {
            var bits = bitMask?
                .GetBits().ToArray();

            if (bits?.Any() ?? false)
            {
                do
                {
                    foreach (var b in bits)
                    {
                        var result = startDate.AddDays(b);

                        if (result > (endDate ?? DateTime.MaxValue))
                            yield break;

                        yield return result;
                    }

                    startDate = startDate.AddDays(bitMask.Length);
                }
                while (startDate <= (endDate ?? DateTime.MinValue));
            }
        }

        public static DateTime GetLastWeekday(this DateTime start, DayOfWeek day)
        {
            var daysToAdd = ((int)day - (int)start.DayOfWeek - 7) % 7;

            return start.AddDays(daysToAdd);
        }

        public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
        {
            var daysToAdd = ((int)day - (int)start.DayOfWeek) % 7;

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

        public static TimeSpan? TimeOfDay(this TimeSpan? value)
        {
            return !value.HasValue
                ? default(TimeSpan?)
                : new TimeSpan(
                    hours: value.Value.Hours,
                    minutes: value.Value.Minutes,
                    seconds: value.Value.Seconds);
        }

        public static string ToBitMask(this IEnumerable<DateTime> dates, DateTime begin, DateTime end)
        {
            var result = new StringBuilder();

            for (DateTime d = begin; d <= end; d = d.AddDays(1))
            {
                var bit = dates.Contains(d) ? "1" : "0";
                result.Append(bit);
            }

            return result.ToString();
        }

        public static string ToBitMask(this IEnumerable<DateTime> dates)
        {
            return dates.ToBitMask(
                begin: dates.Min(),
                end: dates.Max());
        }

        public static string ToBitMask(this IEnumerable<int> bits, int length)
        {
            var result = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                var bit = bits.Contains(i) ? PositiveBit : NegativeBit;
                result.Append(bit);
            }

            return result.ToString();
        }

        public static string ToBitMask(this IEnumerable<int> bits)
        {
            return bits.ToBitMask(bits.Max());
        }

        public static DateTime? ToDate(this string date)
        {
            var result = default(DateTime?);

            if (DateTime.TryParse(date, out DateTime newDate))
            {
                result = newDate;
            }

            return result;
        }

        public static string ToDateString(this DateTime? value, string format = @"yyyy-MM-dd", CultureInfo provider = default)
        {
            return value?.ToString(
                format: format,
                provider: provider ?? CultureInfo.InvariantCulture);
        }

        public static string ToDateString(this DateTime value, string format = @"yyyy-MM-dd")
        {
            return ToDateString(
                value: (DateTime?)value,
                format: format);
        }

        public static TimeSpan? ToTimeSpan(this string input)
        {
            var result = default(TimeSpan?);

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (double.TryParse(input, out double resultValue)
                    && resultValue.ToString(CultureInfo.InvariantCulture) == input.Trim())
                {
                    var current = Convert.ToDouble(
                        value: input,
                        provider: CultureInfo.InvariantCulture);

                    result = DateTime.FromOADate(current) -
                        DateTime.FromOADate(0);
                }
                else
                {
                    result = input.ParseTime();
                }
            }

            return result;
        }

        public static string ToTimeString(this TimeSpan? value, string format = @"hh\:mm\:ss")
        {
            var result = !value.HasValue
                ? default
                : (value?.Ticks < 0 ? "-" : default) + value?.ToString(
                    format: format,
                    formatProvider: CultureInfo.InvariantCulture);

            return result;
        }

        public static string ToTimeString(this TimeSpan value, string format = @"hh\:mm\:ss")
        {
            return ToTimeString(
                value: (TimeSpan?)value,
                format: format);
        }

        #endregion Public Methods

        #region Private Methods

        private static TimeSpan? ParseTime(this string input)
        {
            var result = default(TimeSpan?);

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (timespanPattern.Match(input).Groups["h"].Success
                    && timespanPattern.Match(input).Groups["m"].Success)
                {
                    var days = 0;

                    if (timespanPattern.Match(input).Groups["d1"].Success)
                    {
                        days = int.Parse(
                            s: timespanPattern.Match(input).Groups["d1"].Value,
                            provider: CultureInfo.InvariantCulture);
                    }
                    else if (timespanPattern.Match(input).Groups["d2"].Success)
                    {
                        days = int.Parse(
                            s: timespanPattern.Match(input).Groups["d2"].Value,
                            provider: CultureInfo.InvariantCulture);
                    }

                    var hours = int.Parse(
                        s: timespanPattern.Match(input).Groups["h"].Value,
                        provider: CultureInfo.InvariantCulture);

                    var minutes = int.Parse(
                        s: timespanPattern.Match(input).Groups["m"].Value,
                        provider: CultureInfo.InvariantCulture);

                    var seconds = 0;

                    if (timespanPattern.Match(input).Groups["s"].Success)
                    {
                        seconds = int.Parse(
                            s: timespanPattern.Match(input).Groups["s"].Value,
                            provider: CultureInfo.InvariantCulture);
                    }

                    result = new TimeSpan(
                        days: days,
                        hours: hours,
                        minutes: minutes,
                        seconds: seconds);
                }
            }

            return result;
        }

        #endregion Private Methods
    }
}
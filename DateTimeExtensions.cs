using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Extensions
{
    public static class DateTimeExtensions
    {
        #region Private Fields

        private const char NegativeBit = '0';
        private const char PositiveBit = '1';

        private const string TimeStringPattern =
            @"((?<d1>\d{1,2})\.)?(?<h>\d{1,2})\:(?<m>\d{1,2})(\:(?<s>\d{1,2}))?(\[\+(?<d2>\d)\])?.*";

        #endregion Private Fields

        #region Public Methods

        public static IEnumerable<int> GetBits(this string bitMask)
        {
            if (bitMask?.Any() ?? false)
            {
                var bits = bitMask.ToCharArray();

                for (int i = 0; i < bits.Count(); i++)
                {
                    if (bits[i] == PositiveBit)
                    {
                        yield return i;
                    }
                }
            }
        }

        public static IEnumerable<DateTime> GetDates(
            this string bitMask, DateTime startDate, DateTime? endDate = null)
        {
            var bits = bitMask.GetBits();

            foreach (var b in bits)
            {
                var result = startDate.AddDays(b);

                if (result > (endDate ?? DateTime.MaxValue))
                    yield break;

                yield return startDate.AddDays(b);
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

        public static TimeSpan? TimeOfDay(this TimeSpan? value)
        {
            return !value.HasValue
                ? default(TimeSpan?)
                : new TimeSpan(
                    hours: value.Value.Hours,
                    minutes: value.Value.Minutes,
                    seconds: value.Value.Seconds);
        }

        public static string ToBitMask
            (this IEnumerable<DateTime> dates, DateTime begin, DateTime end)
        {
            var result = new StringBuilder();

            for (DateTime d = begin; d <= end; d = d.AddDays(1))
            {
                var bit = dates.Contains(d) ? "1" : "0";
                result.Append(bit);
            }

            return result.ToString();
        }

        public static string ToBitMask
            (this IEnumerable<DateTime> dates)
        {
            return dates.ToBitMask(
                begin: dates.Min(),
                end: dates.Max());
        }

        public static string ToBitMask
            (this IEnumerable<int> bits, int length)
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

        public static string ToDateString
            (this DateTime? value, string format = @"yyyy-MM-dd")
        {
            return value?.ToString(format);
        }

        public static string ToDateString
            (this DateTime value, string format = @"yyyy-MM-dd")
        {
            return ToDateString(
                value: (DateTime?)value,
                format: format);
        }

        public static TimeSpan? ToTimeSpan
            (this DateTime input)
        {
            return input.TimeOfDay;
        }

        public static TimeSpan? ToTimeSpan
            (this string input)
        {
            var result = default(TimeSpan?);

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (double.TryParse(input, out double resultValue)
                    && resultValue.ToString() == input.Trim())
                {
                    result =
                        DateTime.FromOADate(Convert.ToDouble(input)) -
                        DateTime.FromOADate(0);
                }
                else
                {
                    result = input.ParseTime();
                }
            }

            return result;
        }

        public static TimeSpan? ToTimeSpanUTC
            (this DateTime input)
        {
            return DateTime.Now.Date
                .AddTicks(input.Ticks).ToUniversalTime()
                .Subtract(DateTime.Now.Date);
        }

        public static string ToTimeString
            (this TimeSpan? value, string format = @"hh\:mm\:ss")
        {
            var result = value.HasValue
                ? (value?.Ticks < 0 ? "-" : null) + value?.ToString(format)
                : null;

            return result;
        }

        public static string ToTimeString
            (this TimeSpan value, string format = @"hh\:mm\:ss")
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
                var regex = new Regex(
                    pattern: TimeStringPattern,
                    options: RegexOptions.IgnoreCase);

                if (regex.Match(input).Groups["h"].Success
                    && regex.Match(input).Groups["m"].Success)
                {
                    var days = 0;

                    if (regex.Match(input).Groups["d1"].Success)
                    {
                        days = int.Parse(regex.Match(input).Groups["d1"].Value);
                    }
                    else if (regex.Match(input).Groups["d2"].Success)
                    {
                        days = int.Parse(regex.Match(input).Groups["d2"].Value);
                    }

                    var hours = int.Parse(regex.Match(input).Groups["h"].Value);

                    var minutes = int.Parse(regex.Match(input).Groups["m"].Value);

                    var seconds = regex.Match(input).Groups["s"].Success
                        ? int.Parse(regex.Match(input).Groups["s"].Value)
                        : 0;

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
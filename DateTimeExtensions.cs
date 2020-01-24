﻿using System;
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

        private static readonly Regex timespanPattern =
            new Regex(@"((?<d1>\d{1,2})\.)?(?<h>\d{1,2})\:(?<m>\d{1,2})(\:(?<s>\d{1,2}))?(\[\+(?<d2>\d)\])?.*");

        #endregion Private Fields

        #region Public Methods

        public static TimeSpan? GetAbsoluteSpan(this TimeSpan? from, TimeSpan? to)
        {
            var result = default(TimeSpan?);

            if (from.HasValue && to.HasValue)
            {
                var ticks = Math.Abs(to.Value.Subtract(from.Value).Ticks);
                result = new TimeSpan(ticks);
            }

            return result;
        }

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

        public static IEnumerable<DateTime> GetDates(this string bitMask, DateTime startDate, DateTime? endDate = null)
        {
            var bits = bitMask.GetBits().ToArray();

            if (bits.Any())
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

        public static TimeSpan? GetPart(this TimeSpan? fullDuration, decimal? partLength, decimal? fullLength)
        {
            var result = default(TimeSpan?);

            if ((fullDuration?.Ticks ?? 0) > 0
                && (partLength ?? 0) > 0
                && (fullLength ?? 0) > 0)
            {
                var partTicks = fullDuration.Value.Ticks * ((double)partLength / (double)fullLength);
                result = new TimeSpan((long)partTicks);
            }

            return result;
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

        public static string ToDateString(this DateTime? value, string format = @"yyyy-MM-dd")
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

        public static TimeSpan? ToTimeSpan(this string input)
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

        public static string ToTimeString(this TimeSpan? value, string format = @"hh\:mm\:ss")
        {
            var result = value.HasValue
                ? (value?.Ticks < 0 ? "-" : null) + value?.ToString(format)
                : null;

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
                        days = int.Parse(timespanPattern.Match(input).Groups["d1"].Value);
                    }
                    else if (timespanPattern.Match(input).Groups["d2"].Success)
                    {
                        days = int.Parse(timespanPattern.Match(input).Groups["d2"].Value);
                    }

                    var hours = int.Parse(timespanPattern.Match(input).Groups["h"].Value);

                    var minutes = int.Parse(timespanPattern.Match(input).Groups["m"].Value);

                    var seconds = timespanPattern.Match(input).Groups["s"].Success
                        ? int.Parse(timespanPattern.Match(input).Groups["s"].Value)
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
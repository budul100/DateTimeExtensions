﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DateTimeExtensions
{
    public static class TimeSpanExtensions
    {
        #region Private Fields

        private static readonly Regex timespanRegex =
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

        public static TimeSpan? TimeOfDay(this TimeSpan? value)
        {
            var result = default(TimeSpan?);

            if (value.HasValue)
            {
                result = new TimeSpan(
                    hours: value.Value.Hours,
                    minutes: value.Value.Minutes,
                    seconds: value.Value.Seconds);
            }

            return result;
        }

        public static TimeSpan? ToTimeSpan(this string input, string delimiters = default)
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
                else if (!string.IsNullOrWhiteSpace(delimiters))
                {
                    var delimitersEscaped = Regex.Escape(
                        str: delimiters);

                    var delimitersPattern = $@"(?<h>\d{{1,2}})[{delimitersEscaped}](?<m>\d{{1,2}})([{delimitersEscaped}](?<s>\d{{1,2}}))?";
                    var delimitersRegex = new Regex(
                        pattern: delimitersPattern);

                    result = input.ParseTime(
                        regex: delimitersRegex);
                }
                else
                {
                    result = input.ParseTime(
                        regex: timespanRegex);
                }
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static TimeSpan? ParseTime(this string input, Regex regex)
        {
            var result = default(TimeSpan?);

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (regex.Match(input).Groups["h"].Success
                    && regex.Match(input).Groups["m"].Success)
                {
                    var days = 0;

                    if (regex.Match(input).Groups["d1"].Success)
                    {
                        days = int.Parse(
                            s: regex.Match(input).Groups["d1"].Value,
                            provider: CultureInfo.InvariantCulture);
                    }
                    else if (regex.Match(input).Groups["d2"].Success)
                    {
                        days = int.Parse(
                            s: regex.Match(input).Groups["d2"].Value,
                            provider: CultureInfo.InvariantCulture);
                    }

                    var hours = int.Parse(
                        s: regex.Match(input).Groups["h"].Value,
                        provider: CultureInfo.InvariantCulture);

                    var minutes = int.Parse(
                        s: regex.Match(input).Groups["m"].Value,
                        provider: CultureInfo.InvariantCulture);

                    var seconds = 0;

                    if (regex.Match(input).Groups["s"].Success)
                    {
                        seconds = int.Parse(
                            s: regex.Match(input).Groups["s"].Value,
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
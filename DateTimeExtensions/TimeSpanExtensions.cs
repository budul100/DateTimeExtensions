using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DateTimeExtensions
{
    public static class TimeSpanExtensions
    {
        #region Private Fields

        private const string MeridiemPattern = @"(\s*(?<ampm>[AaPp])\.?[Mm]\.?)?";

        private static readonly Regex timespanRegex = new Regex(
            @"(?:((?<h>\d{2})(?<m>\d{2})(?<s>\d{2})?)|(((?<d1>\d{1,2})\.)?(?<h>\d{1,2})\:(?<m>\d{1,2})(\:(?<s>\d{1,2}))?(\[\+(?<d2>\d)\])?))"
            + MeridiemPattern);

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
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (double.TryParse(
                    s: input,
                    style: NumberStyles.Any,
                    provider: CultureInfo.CurrentCulture,
                    result: out double current)
                    && current.ToString(CultureInfo.CurrentCulture) == input.Trim()
                    && (current < 1 || current % 1 != 0))
                {
                    return DateTime.FromOADate(current) - DateTime.FromOADate(0);
                }

                if (double.TryParse(
                    s: input,
                    style: NumberStyles.Any,
                    provider: CultureInfo.InvariantCulture,
                    result: out double invariant)
                    && invariant.ToString(CultureInfo.InvariantCulture) == input.Trim()
                    && (invariant < 1 || invariant % 1 != 0))
                {
                    return DateTime.FromOADate(invariant) - DateTime.FromOADate(0);
                }

                if (!string.IsNullOrWhiteSpace(delimiters))
                {
                    var delimitersEscaped = Regex.Escape(
                        str: delimiters);

                    var delimitersPattern =
                        $@"(?<h>\d{{1,2}})[{delimitersEscaped}](?<m>\d{{1,2}})([{delimitersEscaped}](?<s>\d{{1,2}}))?";
                    var delimitersRegex = new Regex(
                        pattern: delimitersPattern);

                    return input.ParseTime(
                        regex: delimitersRegex);
                }

                return input.ParseTime(
                    regex: timespanRegex);
            }

            return default;
        }

        #endregion Public Methods

        #region Private Methods

        private static int? GetInt(this Match match, string groupName)
        {
            var group = match.Groups[groupName];

            return group.Success
                ? int.Parse(
                    s: group.Value,
                    provider: CultureInfo.InvariantCulture)
                : default(int?);
        }

        private static TimeSpan? ParseTime(this string input, Regex regex)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return default;
            }

            var match = regex.Match(input);

            if (!match.Groups["h"].Success
                || !match.Groups["m"].Success)
            {
                return default;
            }

            var days = match.GetInt("d1")
                ?? match.GetInt("d2")
                ?? 0;

            var hours = match.GetInt("h").Value;
            var minutes = match.GetInt("m").Value;
            var seconds = match.GetInt("s") ?? 0;

            if (match.Groups["ampm"].Success)
            {
                // 12-hour clock only allows 1..12
                if (hours < 1 || hours > 12)
                {
                    return default;
                }

                var isPm = char.ToUpperInvariant(match.Groups["ampm"].Value[0]) == 'P';

                // 12 AM -> 0, 12 PM -> 12, 1 PM -> 13
                hours = (hours % 12) + (isPm ? 12 : 0);
            }

            return new TimeSpan(
                days: days,
                hours: hours,
                minutes: minutes,
                seconds: seconds);
        }

        #endregion Private Methods
    }
}
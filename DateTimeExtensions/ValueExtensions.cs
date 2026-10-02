using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DateTimeExtensions
{
    public static class ValueExtensions
    {
        #region Public Fields

        public const char NegativeBit = '0';
        public const char PositiveBit = '1';

        #endregion Public Fields

        #region Public Methods

        public static IEnumerable<int> GetBits(this string bitMask, char positiveBit = PositiveBit)
        {
            if (bitMask == default)
            {
                yield break;
            }

            for (var i = 0; i < bitMask.Length; i++)
            {
                if (bitMask[i] == positiveBit)
                {
                    yield return i;
                }
            }
        }

        public static string ToBitmask(this IEnumerable<DateTime> dates, DateTime begin, DateTime end,
            bool defaultOnEmpty = false, char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            // Compare on date level only, time components must not prevent matches
            var days = dates != default
                ? new HashSet<DateTime>(dates.Select(d => d.Date))
                : default;

            var result = new StringBuilder();

            if (days?.Count > 0)
            {
                for (var date = begin.Date; date <= end.Date; date = date.AddDays(1))
                {
                    result.Append(days.Contains(date)
                        ? positiveBit
                        : negativeBit);
                }
            }

            return defaultOnEmpty && result.Length == 0
                ? default
                : result.ToString();
        }

        public static string ToBitmask(this IEnumerable<DateTime> dates, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var days = dates?.Select(d => d.Date).ToArray();

            if (!(days?.Length > 0))
            {
                return defaultOnEmpty
                    ? default
                    : string.Empty;
            }

            return days.ToBitmask(
                begin: days.Min(),
                end: days.Max(),
                defaultOnEmpty: defaultOnEmpty,
                positiveBit: positiveBit,
                negativeBit: negativeBit);
        }

        public static string ToBitmask(this IEnumerable<int> numbers, int length, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var set = numbers != default
                ? new HashSet<int>(numbers)
                : default;

            var result = new StringBuilder();

            if (set?.Count > 0)
            {
                // Numbers outside [0, length) are ignored
                for (var number = 0; number < length; number++)
                {
                    result.Append(set.Contains(number)
                        ? positiveBit
                        : negativeBit);
                }
            }

            return defaultOnEmpty && result.Length == 0
                ? default
                : result.ToString();
        }

        public static string ToBitmask(this IEnumerable<int> bits, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var indices = bits?.ToArray();

            if (!(indices?.Length > 0))
            {
                return defaultOnEmpty
                    ? default
                    : string.Empty;
            }

            // Bits are zero-based indices, so the mask needs Max + 1 positions
            return indices.ToBitmask(
                length: indices.Max() + 1,
                defaultOnEmpty: defaultOnEmpty,
                positiveBit: positiveBit,
                negativeBit: negativeBit);
        }

        public static string ToDateString(this DateTime? value, string format = "yyyy-MM-dd",
            CultureInfo provider = default)
        {
            return value?.ToDateString(
                format: format,
                provider: provider);
        }

        public static string ToDateString(this DateTime value, string format = "yyyy-MM-dd",
            CultureInfo provider = default)
        {
            return value.Date.ToString(
                format: format,
                provider: provider ?? CultureInfo.InvariantCulture);
        }

        public static string ToTimeString(this TimeSpan? value, string format = @"hh\:mm\:ss")
        {
            if (!value.HasValue)
            {
                return default;
            }

            // Custom TimeSpan formats never emit a sign, so add it manually
            var sign = value.Value < TimeSpan.Zero
                ? "-"
                : string.Empty;

            return sign + value.Value.ToString(
                format: format,
                formatProvider: CultureInfo.InvariantCulture);
        }

        public static string ToTimeString(this TimeSpan value, string format = @"hh\:mm\:ss")
        {
            return ToTimeString(
                value: (TimeSpan?)value,
                format: format);
        }

        public static string ToTotalTimeString(this TimeSpan? value)
        {
            return value?.ToTotalTimeString();
        }

        public static string ToTotalTimeString(this TimeSpan value)
        {
            var duration = value.Duration();

            var sign = value < TimeSpan.Zero
                ? "-"
                : string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1:00}:{2:00}:{3:00}",
                sign,
                (int)duration.TotalHours,
                duration.Minutes,
                duration.Seconds);
        }

        #endregion Public Methods
    }
}
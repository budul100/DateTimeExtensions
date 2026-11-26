using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            var first = begin.Date;
            var length = (end.Date - first).Days + 1;

            var buffer = default(char[]);

            if (dates != default && length > 0)
            {
                // A single pass over the dates, duplicates and order do not matter
                foreach (var date in dates)
                {
                    if (buffer == default)
                    {
                        buffer = CreateBuffer(
                            length: length,
                            negativeBit: negativeBit);
                    }

                    var index = (date.Date - first).Days;

                    if (index >= 0 && index < length)
                    {
                        buffer[index] = positiveBit;
                    }
                }
            }

            return GetResult(
                buffer: buffer,
                defaultOnEmpty: defaultOnEmpty);
        }

        public static string ToBitmask(this IEnumerable<DateTime> dates, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var days = dates?.ToArray();

            if (!(days?.Length > 0))
            {
                return GetResult(
                    buffer: default,
                    defaultOnEmpty: defaultOnEmpty);
            }

            var begin = days[0];
            var end = days[0];

            foreach (var day in days)
            {
                if (day < begin)
                {
                    begin = day;
                }

                if (day > end)
                {
                    end = day;
                }
            }

            return days.ToBitmask(
                begin: begin,
                end: end,
                defaultOnEmpty: defaultOnEmpty,
                positiveBit: positiveBit,
                negativeBit: negativeBit);
        }

        public static string ToBitmask(this IEnumerable<int> numbers, int length, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var buffer = default(char[]);

            if (numbers != default && length > 0)
            {
                foreach (var number in numbers)
                {
                    if (buffer == default)
                    {
                        buffer = CreateBuffer(
                            length: length,
                            negativeBit: negativeBit);
                    }

                    // Numbers outside [0, length) are ignored
                    if (number >= 0 && number < length)
                    {
                        buffer[number] = positiveBit;
                    }
                }
            }

            return GetResult(
                buffer: buffer,
                defaultOnEmpty: defaultOnEmpty);
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

            // Custom TimeSpan formats never emit a sign, so add it manually. Standard formats
            // ("c", "g", "G") are a single character and already emit it themselves.
            var isCustomFormat = format?.Length > 1;

            var sign = isCustomFormat && value.Value < TimeSpan.Zero
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

        #region Private Methods

        private static char[] CreateBuffer(int length, char negativeBit)
        {
            var result = new char[length];

            Array.Fill(
                array: result,
                value: negativeBit);

            return result;
        }

        private static string GetResult(char[] buffer, bool defaultOnEmpty)
        {
            if (buffer != default)
            {
                return new string(buffer);
            }

            return defaultOnEmpty
                ? default
                : string.Empty;
        }

        #endregion Private Methods
    }
}

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
            if (bitMask?.Length > 0)
            {
                var bits = bitMask.ToCharArray();

                for (int i = 0; i < bits.Length; i++)
                {
                    if (bits[i] == positiveBit)
                    {
                        yield return i;
                    }
                }
            }
        }

        public static string ToBitmask(this IEnumerable<DateTime> dates, DateTime begin, DateTime end,
            bool defaultOnEmpty = false, char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var result = new StringBuilder();

            if (dates?.Count() > 0)
            {
                for (var date = begin; date <= end; date = date.AddDays(1))
                {
                    var bit = dates.Contains(date)
                        ? positiveBit
                        : negativeBit;

                    result.Append(bit);
                }
            }

            return defaultOnEmpty && result.Length == 0
                ? default
                : result.ToString();
        }

        public static string ToBitmask(this IEnumerable<DateTime> dates, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var result = default(string);

            if (dates?.Count() > 0)
            {
                result = dates?.ToBitmask(
                    begin: dates?.Min() ?? DateTime.MinValue,
                    end: dates?.Max() ?? DateTime.MinValue,
                    defaultOnEmpty: defaultOnEmpty,
                    positiveBit: positiveBit,
                    negativeBit: negativeBit);
            }

            if (result == default)
            {
                result = defaultOnEmpty
                    ? default
                    : string.Empty;
            }

            return result;
        }

        public static string ToBitmask(this IEnumerable<int> numbers, int length, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var result = new StringBuilder();

            if (numbers?.Count() > 0)
            {
                for (var number = 0; number < length; number++)
                {
                    var bit = numbers.Contains(number)
                        ? positiveBit
                        : negativeBit;

                    result.Append(bit);
                }
            }

            return defaultOnEmpty && result.Length == 0
                ? default
                : result.ToString();
        }

        public static string ToBitmask(this IEnumerable<int> bits, bool defaultOnEmpty = false,
            char positiveBit = PositiveBit, char negativeBit = NegativeBit)
        {
            var result = default(string);

            if (bits?.Count() > 0)
            {
                result = bits.ToBitmask(
                    length: bits.Max(),
                    defaultOnEmpty: defaultOnEmpty,
                    positiveBit: positiveBit,
                    negativeBit: negativeBit);
            }

            if (result == default)
            {
                result = defaultOnEmpty
                    ? default
                    : string.Empty;
            }

            return result;
        }

        public static string ToDateString(this DateTime? value, string format = "yyyy-MM-dd", CultureInfo provider = default)
        {
            return value?.ToDateString(
                format: format,
                provider: provider);
        }

        public static string ToDateString(this DateTime value, string format = "yyyy-MM-dd", CultureInfo provider = default)
        {
            return value.Date.ToString(
                format: format,
                provider: provider ?? CultureInfo.InvariantCulture);
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
    }
}
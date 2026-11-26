using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DateTimeExtensions;

public static partial class TimeSpanExtensions
{
    #region Private Fields

    // Same optional AM/PM suffix as in the generated clock regex below
    private const string MeridiemPattern = @"(?:\s*(?<ampm>[AaPp])\.?[Mm]\.?)?";

    // Exclusive upper limit of valid OLE Automation dates
    private const double OleAutomationMaxDays = 2958466d;

    private static readonly ConcurrentDictionary<string, Regex> delimitersRegexes = new();

    #endregion Private Fields

    #region Public Methods

    public static TimeSpan? AddDays(this TimeSpan? time, int days)
    {
        return time?.AddDays(days);
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
        if (string.IsNullOrWhiteSpace(input))
        {
            return default;
        }

        // Explicit delimiters always win, so "10.05" is a time and not a fraction of a day
        if (!string.IsNullOrWhiteSpace(delimiters))
        {
            return input.ParseTime(
                regex: delimitersRegexes.GetOrAdd(
                    key: delimiters,
                    valueFactory: CreateDelimitersRegex));
        }

        return input.ParseDayFraction()
            ?? input.ParseTime(
                regex: ClockRegex());
    }

    #endregion Public Methods

    #region Private Methods

    // Anchored on purpose: the whole input must be a time. Only ASCII digits are accepted,
    // since \d also matches other Unicode digits which int.Parse cannot handle.
    [GeneratedRegex(
        @"^\s*(?:(?<h>[0-9]{2})(?<m>[0-9]{2})(?<s>[0-9]{2})?|(?:(?<d1>[0-9]{1,2})\.)?(?<h>[0-9]{1,2}):(?<m>[0-9]{1,2})(?::(?<s>[0-9]{1,2}))?(?:\[\+(?<d2>[0-9])\])?)(?:\s*(?<ampm>[AaPp])\.?[Mm]\.?)?\s*$")]
    private static partial Regex ClockRegex();

    private static Regex CreateDelimitersRegex(string delimiters)
    {
        // An alternation instead of a character class, since Regex.Escape does not escape '-' and ']'
        var delimiter = "(?:"
            + string.Join("|", delimiters.Distinct().Select(d => Regex.Escape(d.ToString())))
            + ")";

        var pattern = @"^\s*(?<h>[0-9]{1,2})" + delimiter
            + @"(?<m>[0-9]{1,2})(?:" + delimiter + @"(?<s>[0-9]{1,2}))?"
            + MeridiemPattern
            + @"\s*$";

        return new Regex(
            pattern: pattern);
    }

    private static int? GetInt(this Match match, string groupName)
    {
        var group = match.Groups[groupName];

        return group.Success
            ? int.Parse(
                s: group.Value,
                provider: CultureInfo.InvariantCulture)
            : default(int?);
    }

    private static TimeSpan? ParseDayFraction(this string input)
    {
        // Fractions of a day (OLE Automation date) never contain a colon
        if (input.Contains(':'))
        {
            return default;
        }

        var trimmed = input.Trim();

        return trimmed.ParseDayFraction(CultureInfo.CurrentCulture)
            ?? trimmed.ParseDayFraction(CultureInfo.InvariantCulture);
    }

    private static TimeSpan? ParseDayFraction(this string input, CultureInfo culture)
    {
        // The round trip check rejects inputs which are not a plain number in this culture
        if (double.TryParse(
            s: input,
            style: NumberStyles.Any,
            provider: culture,
            result: out var value)
            && double.IsFinite(value)
            && value >= 0
            && value < OleAutomationMaxDays
            && value.ToString(culture) == input
            && (value < 1 || value % 1 != 0))
        {
            return DateTime.FromOADate(value) - DateTime.FromOADate(0);
        }

        return default;
    }

    private static TimeSpan? ParseTime(this string input, Regex regex)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return default;
        }

        var match = regex.Match(input);

        if (!match.Success
            || !match.Groups["h"].Success
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

        // Hours stay open on purpose, they may exceed 24 for services beyond midnight
        if (minutes > 59 || seconds > 59)
        {
            return default;
        }

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

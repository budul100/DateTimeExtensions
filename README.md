# DateTimeExtensions

[![NuGet](https://img.shields.io/nuget/v/budul.DateTimeExtensions.svg)](https://www.nuget.org/packages/budul.DateTimeExtensions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Extension methods for parsing, generating and transforming date and time data, with a focus on
operating calendars: date lists and periods from strings, day bitmasks, moving dates into periods
and tolerant time parsing.

## Installation

```
dotnet add package budul.DateTimeExtensions
```

Targets `netstandard2.0`, `netstandard2.1`, `net7.0` and `net8.0`.

## Overview

| Area | Methods |
|---|---|
| Date lists and periods | `GetDates`, `GetPeriods` |
| Bitmasks | `GetDates` (from bitmask), `GetBits`, `ToBitmask` |
| Period handling | `MoveInPeriod`, `Shift`, `GetNext`, `GetPrevious` |
| Time parsing | `ToTimeSpan` |
| Formatting | `ToDateString`, `ToTimeString`, `ToTotalTimeString` |
| Helpers | `GetAbsDuration`, `AddDays`, `TimeOfDay`, `ToDateTime`, `ToUnspecified` |

## Usage

```csharp
using DateTimeExtensions;
```

### Dates and periods from strings

Sections are separated by `,` (configurable), ranges by `>`.

```csharp
// 2020-01-10, 2020-01-12, 2020-01-13, 2020-01-14
var dates = "2020-01-10,2020-01-12>2020-01-14".GetDates();

// Custom section separator
var other = "2020-01-10;2020-01-12".GetDates(";");

// (2020-01-10 00:00, 2020-01-10 23:59:59.9999999)
// (2020-01-12 18:00, 2020-01-14 10:00)
var periods = "2020-01-10,2020-01-12 18:00>2020-01-14 10:00".GetPeriods();

foreach (var (from, to) in periods)
{
    // ...
}
```

Notes:

* A single date or a range end without time of day covers the whole day.
* Unparsable sections are skipped. A range with a start later than its end throws a `FormatException`.
* `>` cannot be used as section separator (`ArgumentException`).
* Parsing uses `DateTime.TryParse` with the current culture. ISO 8601 (`yyyy-MM-dd`) is recommended.

### Bitmasks

A bitmask is a string where each character represents one day, starting at a given date.

```csharp
// Every Monday between 2020-01-06 and 2020-01-19: 2020-01-06, 2020-01-13
var mondays = "1000000".GetDates(
    startDate: new DateTime(2020, 1, 6),
    endDate: new DateTime(2020, 1, 19));

// Without end date, the mask is applied once
var once = "0001000".GetDates(new DateTime(2020, 1, 6)); // 2020-01-09

// Custom positive character
var custom = "YNY".GetDates(new DateTime(2020, 1, 6), positiveBit: 'Y');

// Dates to bitmask, time of day is ignored: "101"
var mask = new[] { new DateTime(2020, 1, 10), new DateTime(2020, 1, 12) }.ToBitmask();

// Bit indices: { 0, 2 }
var bits = "101".GetBits();

// Indices to bitmask, with explicit length to keep trailing zeros: "0001000"
var fixedLength = new[] { 3 }.ToBitmask(length: 7);
```

Without explicit length, `ToBitmask` ends at the last positive bit. Empty input returns `string.Empty`,
or `null` with `defaultOnEmpty: true`.

### Moving dates into a period

```csharp
var period = new[] { new DateTime(2020, 1, 10), new DateTime(2020, 1, 16) };

// Inside the period: unchanged
new DateTime(2020, 1, 12).MoveInPeriod(period);                  // 2020-01-12

// Non-cyclic: shifted by exactly one period length
new DateTime(2020, 1, 24).MoveInPeriod(period, isCyclic: false); // 2020-01-17

// Cyclic: wrapped into the period
new DateTime(2020, 1, 24).MoveInPeriod(period, isCyclic: true);  // 2020-01-10
```

The time of day of the input is preserved. Overloads exist for `DateTime?` and `IEnumerable<DateTime>`.

### Weekdays

```csharp
var from = new DateTime(2020, 1, 1); // Wednesday

from.GetNext(DayOfWeek.Friday);       // 2020-01-03
from.GetPrevious(DayOfWeek.Monday);   // 2019-12-30
from.GetNext(DayOfWeek.Wednesday);    // 2020-01-01, same day is returned

// All Tuesdays and Fridays in a range
from.GetDates(new DateTime(2020, 1, 31), new[] { DayOfWeek.Tuesday, DayOfWeek.Friday });
```

### Parsing times

`ToTimeSpan` accepts a range of common formats and returns `null` if the input cannot be parsed.

| Input | Result |
|---|---|
| `"13:20"`, `"13:20:55"` | 13:20:00, 13:20:55 |
| `"1320"`, `"132055"` | 13:20:00, 13:20:55 |
| `"12:02:30 AM"`, `"1:15 p.m."` | 00:02:30, 13:15:00 |
| `"1.06:00"` | 1.06:00:00 (day prefix) |
| `"06:00[+1]"` | 1.06:00:00 (day offset suffix) |
| `"0.25399"` | 06:05:44 (OLE Automation fraction of a day) |
| `"10.05.18"` with `ToTimeSpan(".")` | 10:05:18 (custom delimiters) |

### Formatting

```csharp
new DateTime(2020, 1, 10, 8, 0, 0).ToDateString();   // "2020-01-10"

TimeSpan.FromMinutes(-30).ToTimeString();             // "-00:30:00"

// ToTimeString uses the hour component and drops days
new TimeSpan(1, 1, 10, 0).ToTimeString();             // "01:10:00"

// ToTotalTimeString uses total hours, e.g. for service days beyond midnight
new TimeSpan(1, 1, 10, 0).ToTotalTimeString();        // "25:10:00"
```

## License

[MIT](LICENSE)
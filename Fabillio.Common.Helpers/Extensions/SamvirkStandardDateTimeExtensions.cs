using System;
using System.Globalization;
using Fabillio.Common.Helpers.Implementations;

namespace Fabillio.Common.Helpers.Extensions;

public static class SamvirkStandardDateTimeExtensions
{
    private static readonly string _norwayDateFormat = "dd.MM.yyyy";

    private static readonly string _norwayBankIdBirthDate = "ddMMyy";

    private static readonly string _norwayDateTimeFormat = "dd.MM.yyyy hh:mm tt";

    private static readonly string _softrigFormat = "yyyy-MM-dd";

    public static int GetYearsDiff(this DateTime from, DateTime to)
    {
        var yearsDiff = to.Year - from.Year;

        if (from.Date > to.AddYears(-yearsDiff))
        {
            yearsDiff--;
        }

        return yearsDiff;
    }

    public static int MonthDifference(this DateTime from, DateTime to)
        => Math.Abs((from.Month - to.Month) + 12 * (from.Year - to.Year));

    /// <summary>
    /// Softrig/UniEconomy extension
    /// </summary>
    /// <param name="dateTimeStr"></param>
    /// <returns></returns>
    public static string ToSoftrigFormat(this DateTime dateTime)
        => dateTime.ToString(_softrigFormat);

    /// <summary>
    /// Softrig/UniEconomy extension
    /// </summary>
    /// <param name="dateTimeStr"></param>
    /// <returns></returns>
    public static DateTime? FromSoftrigDate(this string dateTimeStr)
    {
        if (DateTime.TryParseExact(dateTimeStr, _softrigFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
        {
            return dateTime;
        }

        return null;
    }

    /// <summary>
    /// Set time to 23:59:59
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime SetMaximumTime(this DateTime dateTime)
        => dateTime.AddDays(1).Date.AddSeconds(-1);

    /// <summary>
    /// A date that is 3 months to 15 months ahead is considered 1 year  
    /// </summary>
    public static int GetYearsUntil(this DateTime start, DateTime until)
    {
        var yearsDiff = until.Year - start.Year;
        if (start.AddMonths(3) < until.AddYears(-yearsDiff))
        {
            yearsDiff++;
        }
        return yearsDiff;
    }

    public static DateTime GetFirstDayMonthFromDate(this DateTime fromDate)
        => new(fromDate.AddMonths(1).Year, fromDate.AddMonths(1).Month, 1);

    public static string ToNorwayDateFormat(this DateTime? dateTime)
    {
        var date = dateTime ?? DateTime.MinValue;
        return date == DateTime.MinValue ? null : date.ToString(_norwayDateFormat);
    }

    public static string ToNorwayBankIdBirthDate(this DateTime dateTime)
    {
        return dateTime.ToString(_norwayBankIdBirthDate);
    }

    public static string ToNorwayDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString(_norwayDateFormat);
    }

    public static string ToNorwayDateTimeFormat(this DateTime? dateTime)
    {
        var date = dateTime ?? DateTime.MinValue;
        return date == DateTime.MinValue ? null : date.ToString(_norwayDateTimeFormat);
    }

    public static string ToNorwayDateTimeFormat(this DateTime dateTime)
    {
        return dateTime.ToString(_norwayDateTimeFormat);
    }
    
    public static bool IsBirthdateQualifiedForYoungMembership(this DateTime birthDate)
    {
        DateTime minimumAllowableDate = new(1995, 01, 01);

        var ageThisYear = DateTimeProvider.Current.UtcNow.Year - birthDate.Year;

        return ageThisYear >= 16 && birthDate >= minimumAllowableDate;
    }

    public static bool IsLongerThan(this DateTime fromDate, Periodicity periodicity, int periodNumber)
    {
        return periodicity switch
        {
            Periodicity.Hours => fromDate >= fromDate.AddHours(periodNumber),
            Periodicity.Days => fromDate >= fromDate.AddDays(periodNumber),
            Periodicity.Months => fromDate >= fromDate.AddMonths(periodNumber),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Determines whether a certain amount of time has elapsed between a given 
    /// </summary>
    /// <param name="fromDate"></param>
    /// <param name="elapsedTimeThreshold"></param>
    /// <param name="elapsedTimeUnit"></param>
    /// <param name="elapsedTime"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool HasElapsedTime(DateTime fromDate, int elapsedTimeThreshold, Periodicity elapsedTimeUnit, out int elapsedTime)
    {
        if (fromDate > DateTime.UtcNow)
        {
            elapsedTime = 0;
            return false;
        }

        TimeSpan timeDifference = DateTime.UtcNow - fromDate;

        elapsedTime = elapsedTimeUnit switch
        {
            Periodicity.Days => (int)timeDifference.TotalDays,
            Periodicity.Hours => (int)timeDifference.TotalHours,
            Periodicity.Minutes => (int)timeDifference.TotalMinutes,
            _ => throw new NotImplementedException($"Invalid time unit '{elapsedTimeUnit}'")
        };

        return elapsedTime >= elapsedTimeThreshold;
    }

    public static bool IsTimePassed(this DateTime startTime, int minutesThreshold, out int minutesLeft)
    {
        if (minutesThreshold <= 0)
        {
            throw new ArgumentException("The minutes threshold must be greater than zero.", nameof(minutesThreshold));
        }

        TimeSpan elapsedTime = DateTime.UtcNow - startTime;
        minutesLeft = (int)Math.Ceiling(Math.Max(minutesThreshold - elapsedTime.TotalMinutes, 0));

        return elapsedTime.TotalMinutes >= minutesThreshold;
    }
}

public enum Periodicity
{
    Hours,
    Days,
    Months,
    Minutes
}
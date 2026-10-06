using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FCS.Utilities;

/// <summary>
/// Provides utility methods for working with DateTime objects, including conversions to and from Unix timestamps,
/// </summary>
public static class DateHelper
{
    #region Date To Unix Timestamp

    /// <summary>
    /// Calculate Unix Time Stamp In UTC
    /// </summary>
    /// <param name="dateTime">DateTime to be converted</param>
    public static long DateTimeToUnixTimestampUtc(this DateTime dateTime) => (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
        new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

    /// <summary>
    /// Calculate Unix Time Stamp In Current Local Time Zone
    /// </summary>
    /// <param name="value">DateTime to be converted</param>
    public static long ToUnixTimeSeconds(this DateTime value) => value.Ticks / 10000000L - 62135596800L;

    #endregion Date To Unix Timestamp

    #region Unix Timestamp To DateTime

    /// <summary>
    /// Convert Unix timestamp To UTC
    /// </summary>
    /// <param name="unixTimeStamp"></param>
    public static DateTime UnixTimeStampToUtc(this long unixTimeStamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp);

    /// <summary>
    /// Convert Unix timestamp to DateTime In LocalTimeZone
    /// </summary>
    /// <param name="unixTimeStamp"></param>
    public static DateTime UnixTimeStampToLocalTimeZone(this long unixTimeStamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();

    #endregion Unix Timestamp To DateTime

    /// <summary>
    /// Return date as short date but with 2 digits for day and month.
    /// For example 01-01-2020.
    /// </summary>
    /// <param name="dt">to format</param>
    /// <returns>formatted date</returns>
    public static string ToShortDateWithFullDigitsString(this DateTime dt)
        => ToShortDateWithFullDigitsString(dt, CultureInfo.CurrentCulture);

    /// <summary>
    /// Return date as short date but with 2 digits for day and month.
    /// For example 01-01-2020.
    /// </summary>
    /// <param name="dt">to format</param>
    /// <param name="culture">to provide the short date pattern for modifying</param>
    /// <returns>formatted date</returns>
    public static string ToShortDateWithFullDigitsString(this DateTime dt, string culture)
        => ToShortDateWithFullDigitsString(dt, new CultureInfo(culture));

    /// <summary>
    /// Return date as short date but with 2 digits for day and month.
    /// For example 01-01-2020.
    /// </summary>
    /// <param name="dt">to format</param>
    /// <param name="culture">to provide the short date pattern for modifying</param>
    /// <returns>formatted date</returns>
    public static string ToShortDateWithFullDigitsString(this DateTime dt, CultureInfo culture)
        => dt.ToString(
            culture.DateTimeFormat.ShortDatePattern
                .Replace("d", "dd")
                .Replace("M", "MM"),
            culture);

    /// <summary>
    /// Return date as long date but without the day name.
    /// For example January 1, 2020.
    /// </summary>
    /// <param name="dt">to format</param>
    /// <returns>formatted date</returns>
    public static string ToLongDateWithoutDayNameString(this DateTime dt)
        => ToLongDateWithoutDayNameString(dt, CultureInfo.CurrentCulture);

    /// <summary>
    /// Return date as long date but without the day name.
    /// For example January 1, 2020.
    /// </summary>
    /// <param name="dt">to format</param>
    /// <param name="culture">to provide the long date pattern for modifying</param>
    /// <returns>formatted date</returns>
    public static string ToLongDateWithoutDayNameString(this DateTime dt, string culture)
        => ToLongDateWithoutDayNameString(dt, new CultureInfo(culture));

    /// <summary>
    /// Return date as long date but without the day name.
    /// For example January 1, 2020.
    /// </summary>
    /// <param name="dt">to format</param>
    /// <param name="culture">to provide the long date pattern for modifying</param>
    /// <returns>formatted date</returns>
    public static string ToLongDateWithoutDayNameString(this DateTime dt, CultureInfo culture)
        => dt.ToString(
            Regex.Replace(
                culture.DateTimeFormat.LongDatePattern, "dddd.", string.Empty).Trim(),
            culture);

    /// <summary>
    /// Converts a Unix timestamp to a DateTime in local time.
    /// </summary>
    /// <param name="unixTimeStamp">The Unix timestamp representing seconds since January 1, 1970, 00:00:00 UTC.</param>
    /// <returns>A DateTime representing the Unix timestamp converted to local time.</returns>
	public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        return dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
    }

    #region Others

    /// <summary>
    /// Find The Requested DayOfWeek With In The Supplied Date Range.
    /// ie. Tell Me All The Saturdays With In The Date Range.
    /// </summary>
    /// <param name="DateRange">Date Range To Be Checked</param>
    /// <param name="dayOfWeek">Day Of Week We Are Looking For</param>
    /// <returns></returns>
    public static List<DateTime>? GetDaysFromRange(List<DateTime> DateRange, DayOfWeek dayOfWeek)
    {
        if (DateRange.Count == 0) return null;                       // Nothing Supplied - Bail

        return DateRange.FindAll(d => d.DayOfWeek == dayOfWeek);    // Return All Matching Days
    }

    #region BusinessDaysBetweenDates

    /// <summary>
    /// Calculate the number of Business Days (Monday through Friday) between two dates.
    /// Holidays are not considered in the calculation.
    /// </summary>
    /// <param name="startDate">First Date</param>
    /// <param name="endDate">Last Date</param>
    public static int BusinessDaysBetweenDates(DateTime startDate, DateTime endDate)
    {
        int businessDays = 0;                               // Initialize Counter

        startDate = startDate.Date;                         // Remove Time Portion
        endDate = endDate.Date;                             // Remove Time Portion

        // Make sure dates are in the correct order
        if (startDate > endDate)                             // If Start Date Is Greater Than End Date
            (endDate, startDate) = (startDate, endDate);    // Swap The Values

        startDate = startDate.AddDays(1);                   // Set startDate To Next Day

        // Working days between dates
        for (DateTime dt = startDate; dt <= endDate; dt = dt.AddDays(1))
        {
            // Don't Include Weekend Days
            if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday) businessDays++;
        }

        return businessDays;
    }

    /// <summary>
    /// BusinessDaysBetweenDates With String Types
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public static int BusinessDaysBetweenDates(string startDate, string endDate)
    {
        DateTime NewStart = DateTime.Parse(startDate, new CultureInfo("en-US"));    // Convert To DateTime
        DateTime NewEnd = DateTime.Parse(endDate, new CultureInfo("en-US"));        // Convert To DateTime

        return BusinessDaysBetweenDates(NewStart, NewEnd);                          // Call Main Method
    }

    /// <summary>
    /// BusinessDaysBetweenDates With DateOnly Types
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public static int BusinessDaysBetweenDates(DateOnly startDate, DateOnly endDate)
    {
        DateTime NewStart = new(startDate.Year, startDate.Month, startDate.Day);          // Convert To DateTime
        DateTime NewEnd = new(endDate.Year, endDate.Month, endDate.Day);                  // Convert To DateTime

        return BusinessDaysBetweenDates(NewStart, NewEnd);                                // Call Main Method
    }

    #endregion BusinessDaysBetweenDates

    #endregion Others
}
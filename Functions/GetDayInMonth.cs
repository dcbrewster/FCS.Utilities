using System;

namespace FCS.Utilities;

public static partial class Function
{
    private const int MinimumYear = 1970;
    private const int MinWeekCount = 1;
    private const int MaxWeekCount = 5;
    private const int DaysInWeek = 7;

    /// <summary>
    /// Find the date of the 1st to 5th occurrence of the requested day.
    /// IE: Get 3rd Monday of month.
    /// </summary>
    /// <param name="yearIn">Year to be used (minimum 1970)</param>
    /// <param name="monthIn">Month to be used (1-12)</param>
    /// <param name="dayIn">What day name are you looking for?</param>
    /// <param name="weekCount">nth Occurrence of day (1-5)</param>
    /// <returns>ShortDate of Occurrence or null if invalid/not found</returns>
    public static string? GetDayInMonth(int yearIn = MinimumYear, int monthIn = 1, DayOfWeek dayIn = DayOfWeek.Monday, int weekCount = 1)
    {
        // Bounds Checking
        if (yearIn < MinimumYear) yearIn = DateTime.Now.Year;
        if (monthIn is < 1 or > 12) monthIn = 1;
        if (dayIn < DayOfWeek.Sunday || dayIn > DayOfWeek.Saturday) dayIn = DayOfWeek.Monday;
        if (weekCount is < MinWeekCount or > MaxWeekCount) weekCount = 1;

        int firstDayOfMonth = (int)new DateTime(yearIn, monthIn, 1).DayOfWeek;
        int offset = (int)dayIn - firstDayOfMonth + 1;

        if (offset < 0) offset += DaysInWeek;

        int thisOccurrence = offset + DaysInWeek * (weekCount - 1);

        try
        {
            return new DateTime(yearIn, monthIn, thisOccurrence).ToString("d");
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
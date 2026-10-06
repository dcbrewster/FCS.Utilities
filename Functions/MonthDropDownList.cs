using System.Collections.Generic;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Return List of SelectListItem containing month value and month name.
    /// Uses standard month numbering (1-12) where 1 = January, 12 = December.
    /// </summary>
    /// <param name="monthToSelect">Month number (1-12) to be marked "Selected". 0 or invalid values select nothing.</param>
    public static List<SelectListItem> MonthDropDownList(int monthToSelect = 0)
    {
        List<SelectListItem> dateList = [];                 // Initialize empty list of SelectListItem

        foreach(MonthName month in MonthList)              // Loop through each month in the MonthList
        {
            dateList.Add(new SelectListItem()               // Add a new SelectListItem to the dateList for each month
            {
                Value = month.Value.ToString(),             // Set the Value to the month's numeric value as a string
                Text = month.Name,                          // Set the Text to the month's full name
                Selected = month.Value == monthToSelect     // Mark as Selected if the month's value matches the monthToSelect parameter
            });
        }

        return dateList;                                    // Return the populated list of SelectListItem representing the months
    }

    /// <summary>
    /// Get List of Month Full Names (January through December)
    /// </summary>
    public static List<string> MonthNamesList => MonthList.ConvertAll(p => p.Name);

    /// <summary>
    /// Get List of Month Short Names (Jan through Dec)
    /// </summary>
    public static List<string> MonthShortNamesList => MonthList.ConvertAll(p => p.ShortName);

    /// <summary>
    /// Return List of Month Value (1-12), Month Name.
    /// Standard month numbering: 1 = January, 12 = December.
    /// </summary>
    public static readonly List<MonthName> MonthList =
    [
        new MonthName { Value = 1, Name = "January", ShortName = "Jan" },
        new MonthName { Value = 2, Name = "February", ShortName = "Feb" },
        new MonthName { Value = 3, Name = "March", ShortName = "Mar" },
        new MonthName { Value = 4, Name = "April", ShortName = "Apr" },
        new MonthName { Value = 5, Name = "May", ShortName = "May" },
        new MonthName { Value = 6, Name = "June", ShortName = "Jun" },
        new MonthName { Value = 7, Name = "July", ShortName = "Jul" },
        new MonthName { Value = 8, Name = "August", ShortName = "Aug" },
        new MonthName { Value = 9, Name = "September", ShortName = "Sep" },
        new MonthName { Value = 10, Name = "October", ShortName = "Oct" },
        new MonthName { Value = 11, Name = "November", ShortName = "Nov" },
        new MonthName { Value = 12, Name = "December", ShortName = "Dec" }
    ];

    /// <summary>
    /// Represents a month with its numeric value (1-12) and names
    /// </summary>
    public class MonthName
    {
        /// <summary>
        /// Month Value (1 = January, 12 = December)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Month's Full Name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Month's Short Name
        /// </summary>
        public string ShortName { get; set; } = string.Empty;
    }
}
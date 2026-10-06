using System.Collections.Generic;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Return a list of SelectListItem containing day value and day name
    /// </summary>
    /// <param name="dayToSelect">int of day to be marked "Selected"</param>
    public static List<SelectListItem> DayDropDownList(int dayToSelect = 0)
    {
        List<SelectListItem> dateList = [];

        foreach (var day in DayList)
        {
            dateList.Add(new SelectListItem
            {
                Value = day.Value.ToString(),
                Text = day.Name,
                Selected = day.Value == dayToSelect
            });
        }

        return dateList;
    }

    /// <summary>
    /// Return List of Day Names
    /// </summary>
    public static List<string> DayNamesList => DayList.ConvertAll(p => p.Name);

    /// <summary>
    /// Return List of Short Day Names
    /// </summary>
    public static List<string> DayShortNamesList => DayList.ConvertAll(p => p.ShortName);

    /// <summary>
    /// List of Days with Values and Names
    /// </summary>
    public static readonly List<DayName> DayList =
    [
        new() { Value = 0, Name = "Sunday", ShortName = "Sun" },
        new() { Value = 1, Name = "Monday", ShortName = "Mon" },
        new() { Value = 2, Name = "Tuesday", ShortName = "Tue" },
        new() { Value = 3, Name = "Wednesday", ShortName = "Wed" },
        new() { Value = 4, Name = "Thursday", ShortName = "Thu" },
        new() { Value = 5, Name = "Friday", ShortName = "Fri" },
        new() { Value = 6, Name = "Saturday", ShortName = "Sat" }
    ];

    #region Sub Classes

    /// <summary>
    /// DayName Class
    /// </summary>
    public class DayName
    {
        /// <summary>
        /// Day Value (0=Sunday, 6=Saturday)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Full English Day Name
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Short English Day Name
        /// </summary>
        public required string ShortName { get; set; }
    }

    #endregion Sub Classes
}
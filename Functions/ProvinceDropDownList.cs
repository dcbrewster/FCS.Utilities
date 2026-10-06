using System;
using System.Collections.Generic;
using System.Linq;
using FCS.Utilities.Models;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Return List of SelectListItem containing Province Names
    /// </summary>
    /// <param name="province">Province to be marked "Selected"</param>
    public static List<SelectListItem> ProvinceDropDownList(string province = "")
    {
        List<SelectListItem> ProvList = [];                                     // List To Hold Provinces

        province = string.IsNullOrEmpty(province) ? "" : province.ToUpper();    // To Upper Case If Not Null

        foreach(Territory p in Provinces)                                       // Process Each Province
        {
            ProvList.Add(new SelectListItem()                                   // Add SelectListItem
            {
                Value = p.ID,                                                   // Set Value
                Text = p.Name,                                                  // Set Text
                Group = p.Group,                                                // Set Group

                // Mark Selected If Required
                Selected = string.Equals(p.ID, province, StringComparison.Ordinal)
                    || string.Equals(p.Name.ToUpper(), province, StringComparison.Ordinal)
            });
        }

        return ProvList;                                                        // Return List of SelectListItem
    }

    /// <summary>
    /// Return List of Province Names
    /// </summary>
    public static List<string> ProvinceList => Provinces.ConvertAll(p => p.Name);

    /// <summary>
    /// Return List of Short (2 characters) Province Names
    /// </summary>
    public static List<string> ProvinceListShortNames => Provinces.ConvertAll(p => p.ID);

    /// <summary>
    /// Build Province List
    /// </summary>
    public static readonly List<Territory> Provinces =
    [
        new Territory { ID = "AB", Name = "Canada - Alberta", Group = "Province" },
        new Territory { ID = "BC", Name = "Canada - British Columbia", Group = "Province" },
        new Territory { ID = "MB", Name = "Canada - Manitoba", Group = "Province" },
        new Territory { ID = "NB", Name = "Canada - New Brunswick", Group = "Province" },
        new Territory { ID = "NL", Name = "Canada - Newfoundland and Labrador", Group = "Province" },
        new Territory { ID = "NT", Name = "Canada - Northwest Territories", Group = "Territory" },
        new Territory { ID = "NS", Name = "Canada - Nova Scotia", Group = "Province" },
        new Territory { ID = "NU", Name = "Canada - Nunavut", Group = "Territory" },
        new Territory { ID = "ON", Name = "Canada - Ontario", Group = "Province" },
        new Territory { ID = "PE", Name = "Canada - Prince Edward Island", Group = "Province" },
        new Territory { ID = "QC", Name = "Canada - Quebec", Group = "Province" },
        new Territory { ID = "SK", Name = "Canada - Saskatchewan", Group = "Province" },
        new Territory { ID = "YT", Name = "Canada - Yukon", Group = "Territory" }
    ];
}
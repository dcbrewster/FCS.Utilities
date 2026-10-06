using System;
using System.Collections.Generic;
using System.Linq;
using FCS.Utilities.Models;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Return List of SelectListItem containing State Names
    /// </summary>
    /// <param name="state">State to be marked "Selected"</param>
    public static List<SelectListItem> StateDropDownList(string state = "")
    {
        List<SelectListItem> StateList = [];                        // List To Hold States

        state = string.IsNullOrEmpty(state) ? "" : state.ToUpper(); // To Upper Case If Not Null

        foreach(Territory s in States)                              // Process Each State
        {
            StateList.Add(new SelectListItem()                      // Add SelectListItem
            {
                Value = s.ID,                                       // Set Value
                Text = s.Name,                                      // Set Text
                Group = s.Group,                                    // Set Group

                // Mark Selected If Required
                Selected = string.Equals(s.ID, state, StringComparison.Ordinal)
                    || string.Equals(s.Name.ToUpper(), state, StringComparison.Ordinal)
            });
        }

        return StateList;                                           // Return List of SelectListItem
    }

    /// <summary>
    /// Return List of State Names
    /// </summary>
    public static List<string> StateList => States.ConvertAll(s => s.Name);

    /// <summary>
    /// Return List of Short (2 characters) State Names
    /// </summary>
    public static List<string> StateListShortNames => States.ConvertAll(s => s.ID);

    /// <summary>
    /// Build States List
    /// </summary>
    public static readonly List<Territory> States =
    [
        new Territory { ID = "AL", Name = "US - Alabama", Group = "State" },
        new Territory { ID = "AK", Name = "US - Alaska", Group = "State" },
        new Territory { ID = "AZ", Name = "US - Arizona", Group = "State" },
        new Territory { ID = "AR", Name = "US - Arkansas", Group = "State" },
        new Territory { ID = "CA", Name = "US - California", Group = "State" },
        new Territory { ID = "CO", Name = "US - Colorado", Group = "State" },
        new Territory { ID = "CT", Name = "US - Connecticut", Group = "State" },
        new Territory { ID = "DE", Name = "US - Delaware", Group = "State" },
        new Territory { ID = "FL", Name = "US - Florida", Group = "State" },
        new Territory { ID = "GA", Name = "US - Georgia", Group = "State" },
        new Territory { ID = "HI", Name = "US - Hawaii", Group = "State" },
        new Territory { ID = "ID", Name = "US - Idaho", Group = "State" },
        new Territory { ID = "IL", Name = "US - Illinois", Group = "State" },
        new Territory { ID = "IN", Name = "US - Indiana", Group = "State" },
        new Territory { ID = "IA", Name = "US - Iowa", Group = "State" },
        new Territory { ID = "KS", Name = "US - Kansas", Group = "State" },
        new Territory { ID = "KY", Name = "US - Kentucky", Group = "State" },
        new Territory { ID = "LA", Name = "US - Louisiana", Group = "State" },
        new Territory { ID = "ME", Name = "US - Maine", Group = "State" },
        new Territory { ID = "MD", Name = "US - Maryland", Group = "State" },
        new Territory { ID = "MA", Name = "US - Massachusetts", Group = "State" },
        new Territory { ID = "MI", Name = "US - Michigan", Group = "State" },
        new Territory { ID = "MN", Name = "US - Minnesota", Group = "State" },
        new Territory { ID = "MS", Name = "US - Mississippi", Group = "State" },
        new Territory { ID = "MO", Name = "US - Missouri", Group = "State" },
        new Territory { ID = "MT", Name = "US - Montana", Group = "State" },
        new Territory { ID = "NE", Name = "US - Nebraska", Group = "State" },
        new Territory { ID = "NV", Name = "US - Nevada", Group = "State" },
        new Territory { ID = "NH", Name = "US - New Hampshire", Group = "State" },
        new Territory { ID = "NJ", Name = "US - New Jersey", Group = "State" },
        new Territory { ID = "NM", Name = "US - New Mexico", Group = "State" },
        new Territory { ID = "NY", Name = "US - New York", Group = "State" },
        new Territory { ID = "NC", Name = "US - North Carolina", Group = "State" },
        new Territory { ID = "ND", Name = "US - North Dakota", Group = "State" },
        new Territory { ID = "OH", Name = "US - Ohio", Group = "State" },
        new Territory { ID = "OK", Name = "US - Oklahoma", Group = "State" },
        new Territory { ID = "OR", Name = "US - Oregon", Group = "State" },
        new Territory { ID = "PA", Name = "US - Pennsylvania", Group = "State" },
        new Territory { ID = "RI", Name = "US - Rhode Island", Group = "State" },
        new Territory { ID = "SC", Name = "US - South Carolina", Group = "State" },
        new Territory { ID = "SD", Name = "US - South Dakota", Group = "State" },
        new Territory { ID = "TN", Name = "US - Tennessee", Group = "State" },
        new Territory { ID = "TX", Name = "US - Texas", Group = "State" },
        new Territory { ID = "UT", Name = "US - Utah", Group = "State" },
        new Territory { ID = "VT", Name = "US - Vermont", Group = "State" },
        new Territory { ID = "VA", Name = "US - Virginia", Group = "State" },
        new Territory { ID = "WA", Name = "US - Washington", Group = "State" },
        new Territory { ID = "WV", Name = "US - West Virginia", Group = "State" },
        new Territory { ID = "WI", Name = "US - Wisconsin", Group = "State" },
        new Territory { ID = "WY", Name = "US - Wyoming", Group = "State" },
        new Territory { ID = "DC", Name = "US - District of Columbia", Group = "District" },
        new Territory { ID = "AS", Name = "US - American Samoa", Group = "Territory" },
        new Territory { ID = "GU", Name = "US - Guam", Group = "Territory" },
        new Territory { ID = "MP", Name = "US - Northern Mariana Islands", Group = "Territory" },
        new Territory { ID = "PR", Name = "Puerto Rico", Group = "Territory" },
        new Territory { ID = "VI", Name = "U.S. Virgin Islands", Group = "Territory" }
    ];
}
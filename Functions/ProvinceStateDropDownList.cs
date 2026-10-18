using System;
using System.Collections.Generic;
using System.Linq;
using FCS.Utilities.Models;

namespace FCS.Utilities;

public static partial class Function
{
	/// <summary>
	/// Return List of SelectListItem containing Province and US State Names
	/// </summary>
	/// <param name="territory">Province/State to be marked "Selected"</param>
	public static List<SelectListItem> ProvinceStateDropDownList(string territory = "")
	{
		List<SelectListItem> TerritoryList = [];					// List To Hold Territories

		territory = string.IsNullOrEmpty(territory) ? "" : territory.ToUpper();     // To Upper Case If Not Null

		foreach(Territory t in Territories())										// Process Each territory
		{
			TerritoryList.Add(new SelectListItem()								// Add SelectListItem
			{
				Value = t.ID,                                                       // Set Value
				Text = t.Name,                                                      // Set Text

				// Mark Selected If Required
				Selected = string.Equals(t.ID, territory, StringComparison.Ordinal)
					|| string.Equals(t.Name.ToUpper(), territory, StringComparison.Ordinal)
			});
		}

		return TerritoryList;                                                       // Return List of SelectListItem
	}

	/// <summary>
	/// Return List of Territory Names
	/// </summary>
	public static List<string> TerritoryList => Territories().ConvertAll(t => t.Name);

	/// <summary>
	/// Return List of Short (2 characters) Territory Names
	/// </summary>
	public static List<string> ProvStateListShortNames => Territories().ConvertAll(t => t.ID);

    /// <summary>
    /// Return List of Territory Objects (Provinces and States)
    /// </summary>
	/// <returns>List of Territory Objects</returns>
	private static List<Territory> Territories() => Provinces.Concat(States).ToList();
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FCS.Utilities.DataMapping.Attributes;

namespace FCS.Utilities.DataMapping;

/// <summary>
/// Provides utility methods for retrieving custom attribute information from types and their members.
/// </summary>
public static class AttributeHelper
{
    /// <summary>
    /// Get DataNames Values For Property
    /// </summary>
    /// <param name="type">Class To Be Searched</param>
    /// <param name="propertyName">Property Name We Are Looking For.</param>
    public static List<string> GetDataNames([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string propertyName)
    {
        var propertyInfo = type.GetProperty(propertyName);
        if (propertyInfo == null)
        {
            return [propertyName];   // Return Property Name if not found
        }

        object? property = Array.Find(propertyInfo.GetCustomAttributes(false), x => string.Equals(x.GetType().Name, "DataNamesAttribute", StringComparison.Ordinal));
        //object property = type.GetProperty(propertyName).GetCustomAttributes(false).Where(x => string.Equals(x.GetType().Name, "DataNamesAttribute", StringComparison.Ordinal)).FirstOrDefault();
        //object property = type.GetProperty(propertyName).GetCustomAttributes(false).Where(x => string.Equals(x.GetType().Name, "DataNamesAttribute", StringComparison.Ordinal));

        if(property != null) return ((DataNamesAttribute)property).ValueNames;

        // No DataNames Found
        return [propertyName];   // Return Property Name
    }
}
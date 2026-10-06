using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace FCS.Utilities.DataMapping;

/// <summary>
/// Helper Class To Map DataRow Values To Entity Properties Based On Custom Attributes
/// </summary>
public static class PropertyMapHelper
{
    /// <summary>
    /// DataMapping
    /// </summary>
    /// <param name="type">Class Name</param>
    /// <param name="row">DataRow</param>
    /// <param name="prop">Property Name</param>
    /// <param name="entity">Result Entity</param>
    public static void Map([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, DataRow row, PropertyInfo prop, object entity)
    {
        // Get DataNames For "prop" As Defined In The "type" Class
        List<string> columnNames = AttributeHelper.GetDataNames(type, prop.Name);

        // If No DataNames Defined For "prop" - Add Table Columns
        if (columnNames.Count == 0)
        {
            foreach (object c in row.Table.Columns)                     // Process Each Table Column
                columnNames.Add(c!.ToString()!);                        // Add Column Name to List
        }

        foreach (string columnName in columnNames)                      // Process Each Column
        {
            if (!string.IsNullOrWhiteSpace(columnName) && row.Table.Columns.Contains(columnName))
            {
                object propertyValue = row[columnName];

                if (propertyValue != DBNull.Value)
                {
                    ParsePrimitive(prop, entity, row[columnName]);      // Set
                    break;                                              // Bail
                }
            }
        }
    }

    /// <summary>
    /// Convert Supplied Value To Property Type And Insert In Entity
    /// </summary>
    /// <param name="prop">Property Name</param>
    /// <param name="entity">Object To Receive Value</param>
    /// <param name="value">Property Value To Insert</param>
    private static void ParsePrimitive(PropertyInfo prop, object entity, object? value)
    {
        if (prop.PropertyType == typeof(string))                                                    // Is string?
        {
            prop.SetValue(entity, value?.ToString()?.Trim(), null);
        }
        else if (prop.PropertyType == typeof(char))                                                 // Is char?
        {
            if (value == null)
            {
                prop.SetValue(entity, null, null);
            }
            else
            {
                prop.SetValue(entity, value?.ToString()?[0], null);
            }
        }
        else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))           // Is bool?
        {
            if (value == null)
            {
                prop.SetValue(entity, null, null);
            }
            else
            {
                prop.SetValue(entity, value?.ToString()?.StringToBoolean(), null);
            }
        }
        else if (prop.PropertyType == typeof(long))                                                 // Is long?
        {
            prop.SetValue(entity, long.Parse(value?.ToString()!), null);
        }
        else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))             // Is int?
        {
            if (value == null)
            {
                prop.SetValue(entity, null, null);
            }
            else
            {
                prop.SetValue(entity, int.Parse(value?.ToString()!), null);
            }
        }
        else if (prop.PropertyType == typeof(decimal))                                              // Is decimal?
        {
            prop.SetValue(entity, decimal.Parse(value!?.ToString()!), null);
        }
        else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))       // Is double?
        {
            if (double.TryParse(value?.ToString(), out _)) prop.SetValue(entity, double.Parse(value?.ToString()!), null);
        }
        else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))   // Is DateTime
        {
            if (DateTime.TryParse(value?.ToString(), out DateTime date))
            {
                prop.SetValue(entity, date, null);
            }
            else
            {
                if (DateTime.TryParseExact(value?.ToString(), "MMddyyyy", new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out date)) prop.SetValue(entity, date, null);
            }
        }
#if NET6_0_OR_GREATER
        else if (prop.PropertyType == typeof(DateOnly) || prop.PropertyType == typeof(DateOnly?))   // Is DateOnly?
        {
            bool isValid = DateOnly.TryParse(value!.ToString(), out DateOnly date);

            if (isValid)
            {
                prop.SetValue(entity, date, null);
            }
            else
            {
                isValid = DateOnly.TryParseExact(value.ToString()!, "MMddyyyy", new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out date);

                if (isValid) prop.SetValue(entity, date, null);
            }
        }
#endif
        else if (prop.PropertyType == typeof(Guid))                                                 // Is Guid?
        {
            bool isValid = Guid.TryParse(value?.ToString(), out Guid guid);

            if (isValid)
            {
                prop.SetValue(entity, guid, null);
            }
            else
            {
                isValid = Guid.TryParseExact(value!.ToString(), "B", out guid);

                if (isValid) prop.SetValue(entity, guid, null);
            }
        }
    }
}
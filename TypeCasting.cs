using System;
using System.Globalization;

namespace FCS.Utilities;

/// <summary>
/// Utility class for type casting and conversions, providing methods to safely convert between
/// various data types while handling potential exceptions and edge cases.
/// All methods return default values instead of throwing exceptions.
/// </summary>
public static class TypeCasting
{
    #region Integer Conversions

    /// <summary>
    /// Converts an object to a 32-bit signed integer.
    /// </summary>
    /// <remarks>Converts through double to handle decimal string representations (e.g., "0.00"). Returns 0
    /// instead of throwing an exception on conversion failure.</remarks>
    /// <param name="i">The object to convert.</param>
    /// <returns>The integer representation of the object, or 0 if the conversion fails.</returns>
    public static int ToInt(object? i)
    {
        if (i == null) return 0;

        try
        {
            // Convert to double first to handle decimal strings like "0.00"
            return Convert.ToInt32(Convert.ToDouble(i));
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Converts a nullable integer to a non-nullable integer.
    /// </summary>
    /// <param name="i">The nullable integer to convert.</param>
    /// <returns>The integer value, or 0 if <paramref name="i"/> is null.</returns>
    public static int ToInt(int? i) => i ?? 0;

    /// <summary>
    /// Converts a string to a nullable integer.
    /// </summary>
    /// <param name="i">The string to convert.</param>
    /// <returns>The parsed integer value if the conversion succeeds; otherwise, null.</returns>
    public static int? ToNullableInt(string? i)
        => int.TryParse(i, out int result) ? result : null;

    /// <summary>
    /// Converts a nullable decimal to a nullable int.
    /// </summary>
    /// <param name="i">The nullable decimal value to convert.</param>
    /// <returns>The decimal value cast to an int, or null if <paramref name="i"/> is null.</returns>
    public static int? ToNullableInt(decimal? i) => i.HasValue ? (int)i.Value : null;

    /// <summary>
    /// Converts an object to a nullable integer by parsing its string representation.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>The integer value if the conversion succeeds; otherwise, null.</returns>
    public static int? ToNullableIntFromObject(object? i)
        => i == null ? null : ToNullableInt(i.ToString());

    /// <summary>
    /// Converts a nullable integer to its string representation, or an empty string if null.
    /// </summary>
    /// <param name="i">The nullable integer to convert.</param>
    /// <returns>The string representation of the integer, or an empty string if null.</returns>
    public static string ToStringInt(int? i) => i?.ToString() ?? "";

    #endregion Integer Conversions

    #region Double/Float Conversions

    /// <summary>
    /// Converts the specified object to a double-precision floating-point number.
    /// </summary>
    /// <remarks>If the conversion throws an exception, the method returns 0.0 instead of propagating the
    /// exception.</remarks>
    /// <param name="i">The object to convert.</param>
    /// <returns>A double-precision floating-point number equivalent to the value of the object, or 0.0 if the conversion fails.</returns>
    public static double ToDouble(object? i)
    {
        if (i == null) return 0.0;

        try
        {
            return Convert.ToDouble(i);
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// Converts the specified string to a nullable double.
    /// </summary>
    /// <param name="i">The string to convert.</param>
    /// <returns>The parsed double value, or null if parsing fails.</returns>
    public static double? ToNullableDouble(string? i)
        => string.IsNullOrEmpty(i) ? null : (double.TryParse(i, out double result) ? result : null);

    /// <summary>
    /// Converts a nullable double to its string representation.
    /// </summary>
    /// <param name="i">The nullable double value to convert.</param>
    /// <returns>The string representation of the double value, or an empty string if the value is null.</returns>
    public static string ToStringDouble(double? i) => i?.ToString() ?? "";

    /// <summary>
    /// Converts the specified object to a single-precision floating-point number.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>A single-precision floating-point number, or 0f if the conversion fails.</returns>
    public static float ToFloat(object? i)
    {
        if (i == null) return 0f;

        try
        {
            return Convert.ToSingle(i);
        }
        catch
        {
            return 0f;
        }
    }

    #endregion Double/Float Conversions

    #region Long Conversions

    /// <summary>
    /// Converts the specified object to a 64-bit signed integer.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>A 64-bit signed integer equivalent to <paramref name="i"/>, or 0 if the conversion fails.</returns>
    public static long ToLong(object? i)
    {
        if (i == null) return 0;

        try
        {
            return Convert.ToInt64(i);
        }
        catch
        {
            return 0;
        }
    }

    #endregion Long Conversions

    #region Decimal Conversions

    /// <summary>
    /// Converts an object to a decimal value, returning zero if the conversion fails.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>The decimal representation of the object, or 0.0 if the conversion fails.</returns>
    public static decimal ToDecimal(object? i)
    {
        if (i == null) return 0.0M;

        try
        {
            return Convert.ToDecimal(i);
        }
        catch
        {
            return 0.0M;
        }
    }

    /// <summary>
    /// Converts the string representation of a number to its nullable decimal equivalent.
    /// </summary>
    /// <param name="i">The string to convert.</param>
    /// <returns>The decimal equivalent of the string if conversion succeeds; otherwise, null.</returns>
    public static decimal? ToNullableDecimal(string? i)
        => string.IsNullOrEmpty(i) ? null : (decimal.TryParse(i, out decimal result) ? result : null);

    /// <summary>
    /// Converts a nullable double to a nullable decimal.
    /// </summary>
    /// <param name="i">The nullable double value to convert.</param>
    /// <returns>The decimal equivalent of the value, or null if the value is null.</returns>
    public static decimal? ToNullableDecimal(double? i) => i.HasValue ? (decimal)i.Value : null;

    /// <summary>
    /// Retrieves the underlying value of a nullable type (e.g., int? or decimal?)
    /// if the provided object is of that type; otherwise, returns the original object.
    /// </summary>
    /// <param name="value">The value to unwrap if nullable.</param>
    /// <returns>The unwrapped value for nullable int or decimal, otherwise the original value.</returns>
    public static object? GetValue(object? value)
    {
        if (value == null) return null;

        Type t = value.GetType();

        if (t.IsGenericType)
        {
            // Check if the generic type definition is int? or decimal? and return the underlying value
            if (t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0] == typeof(int))
                return ((int?)value)!.Value;

            if (t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0] == typeof(decimal))
                return ((decimal?)value)!.Value;
        }

        return value;
    }

    /// <summary>
    /// Converts an object to a nullable decimal.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>A nullable decimal if the conversion succeeds; otherwise, null.</returns>
    public static decimal? ToNullableDecimalFromObject(object? i)
        => i == null ? null : ToNullableDecimal(i.ToString());

    /// <summary>
    /// Converts an object value to a formatted decimal string representation.
    /// </summary>
    /// <param name="value">The object to convert to a decimal string.</param>
    /// <param name="format">The format string to apply to the decimal value.</param>
    /// <returns>A formatted string representation of the decimal value, or an empty string if the value is null or cannot be
    /// converted to a decimal.</returns>
    public static string ToStringDecimal(object? value, string format)
    {
        if (value == null) return "";

        decimal? i = ToNullableDecimal(value.ToString());
        return i?.ToString(format) ?? "";
    }

    /// <summary>
    /// Converts a nullable decimal value to its string representation.
    /// </summary>
    /// <param name="i">The nullable decimal value to convert.</param>
    /// <returns>The string representation of the decimal value, or an empty string if the value is null.</returns>
    public static string ToStringDecimal(decimal? i) => i?.ToString() ?? "";

    /// <summary>
    /// Converts a nullable decimal value to its string representation using the specified format, or returns an empty
    /// string if the value is null.
    /// </summary>
    /// <param name="i">The nullable decimal value to convert.</param>
    /// <param name="format">The format string to apply to the decimal value.</param>
    /// <returns>The formatted string representation of the decimal value, or an empty string if the value is null.</returns>
    public static string ToStringDecimal(decimal? i, string format) => i?.ToString(format) ?? "";

    #endregion Decimal Conversions

    #region Boolean Conversions

    /// <summary>
    /// Converts a nullable boolean to a boolean.
    /// </summary>
    /// <param name="val">The nullable boolean value.</param>
    /// <returns>The boolean value, or <see langword="false"/> if <paramref name="val"/> is <see langword="null"/>.</returns>
    public static bool ToBool(bool? val) => val ?? false;

    /// <summary>
    /// Converts an object to a boolean value.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>The boolean representation of the object, or false if the conversion fails.</returns>
    public static bool ToBool(object? i)
    {
        if (i == null) return false;

        try
        {
            return bool.Parse(i.ToString()!);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts an object to a nullable boolean.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>null if the input is null; false if the conversion fails; otherwise, the parsed boolean value.</returns>
    public static bool? ToNullableBool(object? i)
    {
        if (i == null) return null;

        try
        {
            return bool.Parse(i.ToString()!);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts an object to a boolean value by parsing its string representation.
    /// </summary>
    /// <remarks>Supports common boolean representations including "true", "false", "t", "f", "yes", "no",
    /// "y", "n", "1", "0", "na", and "n/a". Comparisons are case-insensitive. Returns <c>false</c> if the input is null
    /// or cannot be converted.</remarks>
    /// <param name="i">The object to convert to a boolean.</param>
    /// <returns><c>true</c> for values representing true; otherwise, <c>false</c>.</returns>
    public static bool ToBoolFromString(object? i)
    {
        if (i == null) return false;

        string? temp = i.ToString();
        if (string.IsNullOrEmpty(temp)) return false;

        try
        {
            return temp.ToLowerInvariant() switch
            {
                var s when s.StartsWith('t') => true,
                var s when s.StartsWith('f') => false,
                "yes" or "y" or "1" => true,
                "no" or "n" or "0" or "na" or "n/a" => false,
                _ => bool.Parse(temp)
            };
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a string representation of a boolean value to "Yes" or blank.
    /// </summary>
    /// <param name="val">The string value to convert.</param>
    /// <returns>"Yes" if the string represents true; otherwise, an empty string.</returns>
    public static string ToStringBool_YesBlank(string val) => ToStringBool_YesBlank(ToBoolFromString(val));

    /// <summary>
    /// Converts a nullable boolean value to a string representation, returning "Yes" for true or an empty string
    /// otherwise.
    /// </summary>
    /// <param name="val">The nullable boolean value to convert.</param>
    /// <returns>"Yes" if the value is true; otherwise, an empty string.</returns>
    public static string ToStringBool_YesBlank(bool? val) => val == true ? "Yes" : "";

    /// <summary>
    /// Converts a string representation of a boolean value to "Yes" or "No".
    /// </summary>
    /// <param name="val">The string value to convert.</param>
    /// <returns>"Yes" or "No" based on the boolean interpretation.</returns>
    public static string ToStringBool_YesNo(string val) => ToStringBool_YesNo(ToBoolFromString(val));

    /// <summary>
    /// Converts a nullable boolean value to a string representation of "Yes" or "No".
    /// </summary>
    /// <param name="val">The nullable boolean value to convert.</param>
    /// <returns>"Yes" if the value is true; otherwise, "No".</returns>
    public static string ToStringBool_YesNo(bool? val) => val == true ? "Yes" : "No";

    #endregion Boolean Conversions

    #region String Conversions

    /// <summary>
    /// Converts an object to its string representation, returning an empty string for null values.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>The string representation of the object, or an empty string if the object is null.</returns>
    public static string ToStringOrEmpty(object? i) => i?.ToString() ?? "";

    /// <summary>
    /// Converts an object to its string representation, returning a specified value if the result is null, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <param name="ifBlankValue">The value to return if the result is blank.</param>
    /// <returns>The string representation of the object, or <paramref name="ifBlankValue"/> if blank.</returns>
    public static string ToStringOrDefault(object? i, string ifBlankValue)
    {
        if (i == null) return ifBlankValue;

        string? result = i.ToString();
        return string.IsNullOrWhiteSpace(result) ? ifBlankValue : result;
    }

    /// <summary>
    /// Converts an object to its string representation if the length does not exceed the specified maximum.
    /// </summary>
    /// <param name="i">The object to convert to a string.</param>
    /// <param name="maxStringLength">The maximum allowed length of the resulting string.</param>
    /// <returns>The string representation of the object if its length is less than <paramref name="maxStringLength"/>;
    /// otherwise, an empty string. Returns an empty string if <paramref name="i"/> is null.</returns>
    public static string ToString(object? i, int maxStringLength)
    {
        if (i == null) return "";

        string result = i.ToString() ?? "";
        return result.Length < maxStringLength ? result : "";
    }

    /// <summary>
    /// Converts an object to its string representation with optional substring extraction and length limiting.
    /// </summary>
    /// <param name="i">The object to convert to a string.</param>
    /// <param name="subStringStart">The starting index from which to extract the substring. Characters before this index are skipped.</param>
    /// <param name="maxStringLength">The maximum length of the resulting string.</param>
    /// <returns>A string representation of the object, optionally trimmed from the start and limited in length, or an empty
    /// string if the object is null.</returns>
    public static string ToString(object? i, int subStringStart, int maxStringLength)
    {
        if (i == null) return "";

        string result = i.ToString() ?? "";
        if (string.IsNullOrEmpty(result)) return "";

        if (result.Length > subStringStart)
            result = result[subStringStart..];

        if (result.Length > maxStringLength)
            result = result[..maxStringLength];

        return result;
    }

    /// <summary>
    /// Converts an object to a string representation with a fixed length, padding with spaces if necessary.
    /// </summary>
    /// <param name="i">The object to convert to a string.</param>
    /// <param name="maxStringLength">The desired fixed length of the resulting string.</param>
    /// <returns>A string representation of the object, padded with trailing spaces to reach the specified length if needed.</returns>
    public static string ToFixedString(object? i, int maxStringLength)
    {
        string result = ToString(i, maxStringLength);
        return result.Length < maxStringLength
            ? result.PadRight(maxStringLength)
            : result;
    }

    #endregion String Conversions

    #region DateTime Conversions

    /// <summary>
    /// Converts an object to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The object to convert.</param>
    /// <returns>A <see cref="DateTime"/> parsed from the string representation of <paramref name="dateTime"/>, or <see cref="DateTime.MinValue"/>
    /// if the conversion fails.</returns>
    public static DateTime ToDateTime(object? dateTime)
    {
        if (dateTime == null) return DateTime.MinValue;

        return DateTime.TryParse(dateTime.ToString(), out DateTime result)
            ? result
            : DateTime.MinValue;
    }

    /// <summary>
    /// Converts an object to a DateTime using the specified format string and invariant culture.
    /// </summary>
    /// <param name="dateTime">The object to convert to DateTime.</param>
    /// <param name="format">The format string used to parse the DateTime value.</param>
    /// <returns>The parsed DateTime value, or <see cref="DateTime.MinValue"/> if the object is null or parsing fails.</returns>
    public static DateTime ToDateTime(object? dateTime, string format)
    {
        if (dateTime == null) return DateTime.MinValue;

        return DateTime.TryParseExact(dateTime.ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
            ? result
            : DateTime.MinValue;
    }

    /// <summary>
    /// Converts an object to a nullable DateTime.
    /// </summary>
    /// <param name="dateTime">The object to convert.</param>
    /// <returns>The parsed DateTime if successful; otherwise, null.</returns>
    public static DateTime? ToNullableDateTime(object? dateTime)
    {
        if (dateTime == null) return null;

        return DateTime.TryParse(dateTime.ToString(), out DateTime result)
            ? result
            : null;
    }

    /// <summary>
    /// Converts an object to a nullable TimeSpan.
    /// </summary>
    /// <param name="timeSpan">The object to convert.</param>
    /// <returns>The parsed TimeSpan value, or null if the object is null or cannot be parsed.</returns>
    public static TimeSpan? ToNullableTimeSpan(object? timeSpan)
    {
        if (timeSpan == null) return null;

        return TimeSpan.TryParse(timeSpan.ToString(), out TimeSpan result)
            ? result
            : null;
    }

    /// <summary>
    /// Converts an object to a nullable <see cref="DateTime"/> using the specified exact format.
    /// </summary>
    /// <param name="dateTime">The object to convert to a <see cref="DateTime"/>.</param>
    /// <param name="format">The exact format string to use when parsing the date and time.</param>
    /// <returns>The parsed <see cref="DateTime"/> value if successful; otherwise, <see langword="null"/> if <paramref
    /// name="dateTime"/> is <see langword="null"/> or parsing fails.</returns>
    public static DateTime? ToNullableDateTime(object? dateTime, string format)
    {
        if (dateTime == null) return null;

        return DateTime.TryParseExact(dateTime.ToString(), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result)
            ? result
            : null;
    }

    /// <summary>
    /// Converts a value to a DateTime and formats it as a string.
    /// </summary>
    /// <param name="value">The value to convert to DateTime.</param>
    /// <param name="format">The format string to use for the DateTime representation.</param>
    /// <returns>The formatted DateTime string, or an empty string if the value cannot be converted to a DateTime.</returns>
    public static string ToStringDateTime(object? value, string format)
    {
        DateTime? dateTime = ToNullableDateTime(value);
        return dateTime?.ToString(format) ?? "";
    }

    /// <summary>
    /// Converts a nullable DateTime to its string representation using the specified format, or returns an empty string
    /// if the value is null.
    /// </summary>
    /// <param name="dateTime">The nullable DateTime value to convert.</param>
    /// <param name="format">The format string to apply to the DateTime value.</param>
    /// <returns>The formatted string representation of the DateTime, or an empty string if <paramref name="dateTime"/> is null.</returns>
    public static string ToStringDateTime(DateTime? dateTime, string format)
        => dateTime?.ToString(format) ?? "";

    /// <summary>
    /// Converts a nullable TimeSpan to a formatted date and time string representation.
    /// </summary>
    /// <param name="dateTime">The TimeSpan value to convert to a DateTime and format.</param>
    /// <param name="format">The format string to apply to the DateTime representation.</param>
    /// <returns>A formatted date and time string, or an empty string if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    public static string ToStringDateTime(TimeSpan? dateTime, string format)
        => dateTime.HasValue ? new DateTime(dateTime.Value.Ticks).ToString(format) : "";

    #endregion DateTime Conversions

    #region Guid Conversions

    /// <summary>
    /// Converts an object to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>A <see cref="Guid"/> parsed from the string representation of <paramref name="i"/>, or <see cref="Guid.Empty"/>
    /// if the conversion fails.</returns>
    public static Guid ToGuid(object? i)
    {
        if (i == null) return Guid.Empty;

        try
        {
            return new Guid(i.ToString()!);
        }
        catch
        {
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Converts an object to a nullable Guid. Returns null if the input is null or if the conversion fails.
    /// </summary>
    /// <param name="i">The object to convert.</param>
    /// <returns>A nullable Guid if the conversion succeeds; otherwise, null.</returns>
    public static Guid? ToNullableGuid(object? i)
    {
        if (i == null) return null;

        try
        {
            return new Guid(i.ToString()!);
        }
        catch
        {
            return null;
        }
    }

    #endregion Guid Conversions

    #region TimeSpan Conversions

    /// <summary>
    /// Converts the string representation of a time interval to its TimeSpan equivalent.
    /// </summary>
    /// <param name="timespan">The string to convert.</param>
    /// <returns>A TimeSpan that represents the time interval, or null if the conversion failed.</returns>
    public static TimeSpan? ToTimeSpan(string? timespan)
    {
        if (string.IsNullOrEmpty(timespan)) return null;

        return TimeSpan.TryParse(timespan, out TimeSpan result)
            ? result
            : null;
    }

    #endregion TimeSpan Conversions
}
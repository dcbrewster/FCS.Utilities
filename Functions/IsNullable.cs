using System;
using System.Linq;
using System.Reflection;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Determine if specified type is nullable
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if type is nullable, false otherwise</returns>
    public static bool IsNullable(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return !type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    /// <summary>
    /// Return underlying type if type is Nullable otherwise return the type
    /// </summary>
    /// <param name="type">Type to unwrap</param>
    /// <returns>Underlying type or original type</returns>
    public static Type GetCoreType(Type? type)
    {
        if (type is not null && IsNullable(type)) return !type.IsValueType ? type : Nullable.GetUnderlyingType(type) ?? type;

        return type ?? typeof(object);
    }

    /// <summary>
    /// Get custom attribute from enum value
    /// </summary>
    /// <typeparam name="TAttribute">Type of attribute to retrieve</typeparam>
    /// <param name="enumValue">Enum value</param>
    /// <returns>Attribute instance or null if not found</returns>
    public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        return enumValue
            .GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<TAttribute>();
    }
}
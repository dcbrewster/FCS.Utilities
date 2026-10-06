using System;
using System.Collections.Generic;

namespace FCS.Utilities.DataMapping.Attributes;

/// <summary>
/// Specifies alternative names for a property's data values.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DataNamesAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataNamesAttribute"/> class.
    /// </summary>
    public DataNamesAttribute() => _valueNames = [];

    /// <summary>
    /// Initializes a new instance of the DataNamesAttribute class with the specified value names.
    /// </summary>
    /// <param name="valueNames">An array of value names.</param>
    public DataNamesAttribute(params string[] valueNames) => _valueNames = [.. valueNames];

    /// <summary>
    /// The list of value names.
    /// </summary>
    public List<string> ValueNames
    {
        get
        {
            return _valueNames;
        }
        set
        {
            _valueNames = value;
        }
    }

    /// <summary>
    /// Gets or sets the list of value names.
    /// </summary>
    protected List<string> _valueNames { get; set; }
}
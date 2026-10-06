namespace FCS.Utilities;

/// <summary>
/// Represents an item in a select list.
/// </summary>
public class SelectListItem
{
    /// <summary>
    /// Gets or sets a value indicating whether the item is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the group name of the item.
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the item is selected.
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// Gets or sets the text of the item.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the value of the item.
    /// </summary>
    public required string Value { get; set; }
}
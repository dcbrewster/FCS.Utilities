namespace FCS.Utilities.Models;

/// <summary>
/// Model To Hold Territory (Province/State) Information
/// </summary>
public class Territory
{
    /// <summary>
    /// Territory ID as defined by the Post Office
    /// </summary>
    public required string ID { get; set; }

    /// <summary>
    /// Territory Name as defined by the Post Office
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// SelectListItem Group
    /// </summary>
    public required string Group { get; set; }
}
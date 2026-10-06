namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Remove The Last Character Of The Supplied String
    /// </summary>
    /// <param name="stringIn">String To Be Modified</param>
    public static string? RemoveLastCharacter(this string? stringIn) => string.IsNullOrEmpty(stringIn) ? stringIn : stringIn[..^1];
}
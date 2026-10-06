using System.IO;
using System.Reflection;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Gets or sets the physical path of the web's root directory.
    /// </summary>
    public static string WebPath
    {
        get => field ?? Directory.GetCurrentDirectory();
        set;
    } = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
}
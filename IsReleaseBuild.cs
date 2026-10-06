namespace FCS.Utilities;

/// <summary>
/// Utility class for process control related functions, such as determining if the current build is a release build.
/// </summary>
public static class ProcessControl
{
    /// <summary>
    /// Determine If The Current Build Is A Release Build
    /// </summary>
    public static bool IsReleaseBuild()
    {
#if DEBUG
        return false;
#else
        return true;
#endif
    }
}
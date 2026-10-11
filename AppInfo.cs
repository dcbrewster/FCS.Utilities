using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FCS.Utilities;

/// <summary>
/// Return Various Properties About Application
/// </summary>
public static class AppInfo
{
    public static readonly string LayoutFileName = $@"{ExePath}\{ExeName}.layout";
    public static readonly string SettingFileName = $@"{ExePath}\{ExeName}.cfg";
    public static readonly string LogFileName = $@"{ExePath}\{ExeName}.log";

    /// <summary>
    /// Return The Path Of The Executable File.
    /// </summary>
    public static string Title => ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

    /// <summary>
    /// Return The Product Of The Executable File.
    /// </summary>
    public static string Product => ((AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;

    /// <summary>
    /// Return The Copyright Of The Executable File.
    /// </summary>
    public static string Copyright => ((AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;

    /// <summary>
    /// Return The Version Of The Executable File.
    /// </summary>
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public static string CompanyName => ((AssemblyCompanyAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0]).Company;

    /// <summary>
    /// Return The Name Of The Executable File.
    /// </summary>
    public static string ExeName => Process.GetCurrentProcess().ProcessName;

    /// <summary>
    /// Return The Path Of The Executable File.
    /// </summary>
    public static string ExePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    /// <summary>
    /// Return The Application Name Of The Executable File (.Net5 And Above)
    /// </summary>
    public static string ApplicationName => ExeName;
}
using System;
using Microsoft.Extensions.Configuration;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Get the value of the application key from appsettings.json, appsettings.{Environment}.json, or Environment Variables.
    /// </summary>
    /// <param name="setting">Name of Application Setting Key</param>
    /// <returns>Configuration value or null if not found.</returns>
    public static string? GetAppSetting(string setting)
    {
        if(string.IsNullOrWhiteSpace(setting)) throw new ArgumentException("Setting name cannot be null or whitespace.", nameof(setting));

        return Configuration.Value[setting];
    }

    /// <summary>
    /// Get the value of the application key, returning the specified default value when the setting is not found.
    /// </summary>
    /// <param name="setting">Name of Application Setting Key</param>
    /// <param name="defaultValue">Value to return when the setting is not found.</param>
    /// <returns>Configuration value or the specified default value.</returns>
    public static string GetAppSetting(string setting, string defaultValue) => GetAppSetting(setting) ?? defaultValue;

    private static readonly Lazy<IConfigurationRoot> Configuration = new(BuildConfiguration);

    private static IConfigurationRoot BuildConfiguration()
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Production";

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
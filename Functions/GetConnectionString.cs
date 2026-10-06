using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Return The Connection String For The Supplied Setting
    /// <para>
    /// WARNING: This method creates a new IConfiguration instance on every call which is expensive.
    /// For production code, inject IConfiguration via dependency injection instead.
    /// </para>
    /// </summary>
    /// <param name="setting">The ConnectionString Key (defaults to "Default")</param>
    /// <returns>Connection string or null if not found</returns>
    public static string? GetConnectionString(string? setting = "Default")
    {
        if (string.IsNullOrWhiteSpace(setting)) setting = "Default";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        return configuration.GetConnectionString(setting);
    }
}
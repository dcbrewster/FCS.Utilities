using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlDataAdapter = Microsoft.Data.SqlClient.SqlDataAdapter;

namespace FCS.Utilities;

/// <summary>
/// Provides utility methods for executing stored procedures and SQL commands against a database,
/// with support for both synchronous and asynchronous operations.
/// </summary>
public static class DBHelper
{
    /// <summary>
    /// The connection string used for database operations.
    /// This is set internally based on the provided connection string key or defaults to the "Default" connection string from the application configuration.
    /// </summary>
    private static string? ConnectionString { get; set; }

    /// <summary>
    /// Return the Database Minimum Value For DateTime Columns
    /// </summary>
    public static DateTime DBMinValue => (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;

    /// <summary>
    /// Executes a SQL stored procedure and returns the results as a DataSet.
    /// </summary>
    /// <remarks>If the specified connection string key is empty or not found, the method attempts to use the
    /// default connection string. The results are populated in a table named "Results".</remarks>
    /// <param name="procName">The name of the stored procedure to execute.</param>
    /// <param name="parameters">The SQL parameters to pass to the stored procedure, or null if no parameters are required.</param>
    /// <param name="timeOut">The command timeout in seconds. Default is 15 seconds.</param>
    /// <param name="connectionStringKey">The configuration key for the connection string. If empty or not found, the default connection string is used.</param>
    /// <returns>A DataSet containing the results of the stored procedure, or an empty DataSet if the procedure name or
    /// connection string is invalid.</returns>
    public static DataSet RunStoredProc(string procName, SqlParameter[]? parameters = null, int timeOut = 15, string connectionStringKey = "")
    {
        // Get ConnectionString For Key
        if (!string.IsNullOrEmpty(connectionStringKey)) ConnectionString = Function.GetConnectionString(connectionStringKey);

        // If No ConnectionString, Try Getting Default
        if (string.IsNullOrEmpty(ConnectionString)) ConnectionString = Function.GetConnectionString("Default");

        // If No Stored Procedure Name Or ConnectionString - Bail
        if (string.IsNullOrEmpty(procName) || string.IsNullOrEmpty(ConnectionString)) return new DataSet();

        DataSet result = new();

        using (SqlConnection con = new(ConnectionString))
        {
            using SqlCommand cmd = new()
            {
                CommandText = procName,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = timeOut
            };

            if (parameters != null) cmd.Parameters.AddRange(parameters);

            try
            {
                con.Open();
                cmd.Connection = con;

                SqlDataAdapter da = new(cmd);

                da.Fill(result, "Results");
            }
            catch (SqlException)
            {
                // Log the exception properly using your logging framework
                // Example: _logger.LogError(ex, "Error executing stored procedure {ProcName}", procName);
                throw; // Re-throw to let caller handle or log at application level
            }
            catch (Exception)
            {
                // Log unexpected exceptions
                // Example: _logger.LogError(ex, "Unexpected error in RunStoredProc");
                throw;
            }
        }

        return result;
    }

    /// <summary>
    /// Execute the defined stored procedure asynchronously.
    /// The ConnectionString will be retrieved from the application configuration as defined by the
    /// supplied connectionStringKey. If the key does not exist, the method will try to retrieve a
    /// ConnectionString with the "Default" key.
    /// </summary>
    /// <param name="procName">Name of Stored Procedure</param>
    /// <param name="parameters">Array of SQL Parameters (Optional)</param>
    /// <param name="timeOut">Number Of Seconds Before TimeOut - default 15 (Optional)</param>
    /// <param name="connectionStringKey">Connection String Key From AppSettings.json (Optional)</param>
    public static async Task<DataSet> RunStoredProcAsync(string procName, SqlParameter[]? parameters = null, int timeOut = 15, string connectionStringKey = "")
    {
        // Get ConnectionString For Key
        if (!string.IsNullOrEmpty(connectionStringKey)) ConnectionString = Function.GetConnectionString(connectionStringKey);

        // If No ConnectionString, Try Getting Default
        if (string.IsNullOrEmpty(ConnectionString)) ConnectionString = Function.GetConnectionString("Default");

        // If No Stored Procedure Name Or ConnectionString - Bail
        if (string.IsNullOrEmpty(procName) || string.IsNullOrEmpty(ConnectionString)) return new DataSet();

        DataSet result = new();

        await using (SqlConnection con = new(ConnectionString))
        {
            await using SqlCommand cmd = new()
            {
                CommandText = procName,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = timeOut
            };

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await con.OpenAsync();
            cmd.Connection = con;

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            result.Load(reader, LoadOption.OverwriteChanges, "Results");
        }

        return result;
    }

    /// <summary>
    /// Execute SQL Command asynchronously
    /// </summary>
    /// <param name="command">Command To Be Executed</param>
    /// <param name="timeOut">Number Of Seconds Before TimeOut - default 30 (Optional)</param>
    /// <param name="connectionStringKey">Connection String Key From AppSettings.json (Optional)</param>
    public static async Task<DataSet> RunSqlCommandAsync(string command, int timeOut = 30, string connectionStringKey = "")
    {
        // Get ConnectionString For Key
        if (!string.IsNullOrEmpty(connectionStringKey)) ConnectionString = Function.GetConnectionString(connectionStringKey);

        // If No ConnectionString, Try Getting Default
        if (string.IsNullOrEmpty(ConnectionString)) ConnectionString = Function.GetConnectionString("Default");

        // If No Command Or ConnectionString - Bail
        if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(ConnectionString)) return new DataSet();

        DataSet result = new();

        await using (SqlConnection con = new(ConnectionString))
        {
            await using SqlCommand cmd = new()
            {
                CommandText = command,
                CommandType = CommandType.Text,
                CommandTimeout = timeOut
            };

            await con.OpenAsync();
            cmd.Connection = con;

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            result.Load(reader, LoadOption.OverwriteChanges, "Results");
        }

        return result;
    }

    /// <summary>
    /// Execute SQL Command
    /// </summary>
    /// <param name="command">Command To Be Executed</param>
    /// <param name="timeOut">Number Of Seconds Before TimeOut - default 30 (Optional)</param>
    /// <param name="connectionStringKey">Connection String Key From AppSettings.json (Optional)</param>
    public static DataSet RunSqlCommand(string command, int timeOut = 30, string connectionStringKey = "")
    {
        // Get ConnectionString For Key
        if (!string.IsNullOrEmpty(connectionStringKey)) ConnectionString = Function.GetConnectionString(connectionStringKey);

        // If No ConnectionString, Try Getting Default
        if (string.IsNullOrEmpty(ConnectionString)) ConnectionString = Function.GetConnectionString("Default");

        // If No Command Or ConnectionString - Bail
        if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(ConnectionString)) return new DataSet();

        DataSet result = new();                                               // Create Empty DataSet Object

        using (SqlConnection con = new(ConnectionString))                 // Build SQLConnection Object
        {
            using SqlCommand cmd = new()                                   // Build SQLCommand Object
            {
                CommandText = command,                                              // Set SQL Command
                CommandType = CommandType.Text,                                     // Set As Text
                CommandTimeout = timeOut                                            // Set Connection Time Out
            };

            con.Open();                                                             // Open The Connection
            cmd.Connection = con;                                                   // Attach The Connection Object To The Command Object
            SqlDataAdapter da = new(cmd);                                           // Execute the Stored Procedure

#if !DEBUG                                                                          // In Production Environment
			// Catch any errors in Production
			try
			{
				da.Fill(result, "Results");                                         // Fill The DataSet
			}
			catch(Exception e)                                                      // Catch Error
			{
				// Report Exception
			}
#else                                                                               // In Development/Testing Environment
            // Abort in Debug Mode
            da.Fill(result, "Results");                                             // Fill The DataSet
#endif
        }

        return result;                                                              // Return The Result DataSet
    }
}
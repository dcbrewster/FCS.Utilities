using System;
using System.Data;
using System.Globalization;

namespace FCS.Utilities;

/// <summary>
/// Provides utility methods for retrieving data from DataRow objects.
/// </summary>
public static class DataHelpers
{
	/// <summary>
	/// Get String From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>String from Row and Column</returns>
	public static string GetDataString(DataRow dataRow, string columnName) => dataRow[columnName] == DBNull.Value ? string.Empty : (string)dataRow[columnName];

	/// <summary>
	/// Get DateTime Value From Supplied DataRow And ColumnName
	/// </summary>
	/// <param name="dataRow">Supplied DataRow</param>
	/// <param name="columnName">Supplied Column Namee</param>
	public static DateTime GetDataDateTime(DataRow dataRow, string columnName) => (DateTime)(dataRow[columnName] == DBNull.Value ? DateTime.MinValue : dataRow[columnName]);

	/// <summary>
	/// Get DateTime2 Value From Supplied DataRow And ColumnName
	/// </summary>
	/// <param name="dataRow">Supplied DataRow</param>
	/// <param name="columnName">Supplied Column Namee</param>
	public static DateTime GetDataDateTime2(DataRow dataRow, string columnName) => (DateTime)(dataRow[columnName] == DBNull.Value ? DateTime.MinValue : dataRow[columnName]);

	/// <summary>
	/// Get Date Only From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>Date Only from Row and Column</returns>
	public static DateOnly? GetDataDateOnly(DataRow dataRow, string columnName) => dataRow[columnName] == DBNull.Value ? null : DateOnly.FromDateTime((DateTime)dataRow[columnName]);

	/// <summary>
	/// Get Boolean From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>Boolean from Row and Column</returns>
	public static bool GetDataBool(DataRow dataRow, string columnName) => dataRow[columnName] != DBNull.Value && (bool)dataRow[columnName];

	/// <summary>
	/// Get Integer From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>Integer from Row and Column</returns>
	public static int GetDataInteger(DataRow dataRow, string columnName) => dataRow[columnName] == DBNull.Value ? 0 : (int)dataRow[columnName];

	/// <summary>
	/// Get Decimal From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>Decimal from Row and Column</returns>
	public static decimal GetDataDecimal(DataRow dataRow, string columnName) => dataRow[columnName] == DBNull.Value ? 0 : (decimal)dataRow[columnName];

	/// <summary>
	/// Get Double From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>Double from Row and Column</returns>
	public static double GetDataDouble(DataRow dataRow, string columnName) => dataRow[columnName] == DBNull.Value ? 0 : (double)dataRow[columnName];

	/// <summary>
	/// Get Float From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>Float from Row and Column</returns>
	public static float GetDataFloat(DataRow dataRow, string columnName) => dataRow[columnName] == DBNull.Value ? 0 : (float)dataRow[columnName];

	/// <summary>
	/// Get Guid From Supplied dataRow and columnName
	/// </summary>
	/// <param name="dataRow">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns>Guid from Row and Column</returns>
	public static Guid GetDataGuid(DataRow dataRow, string columnName) => dataRow[columnName] == DBNull.Value ? Guid.Empty : (Guid)dataRow[columnName];

	/// ToDo: Test
	/// <summary>
	/// Test Using Anonymous
	/// </summary>
	/// <typeparam name="T">Object Type</typeparam>
	/// <param name="row">Row in DataSet</param>
	/// <param name="columnName">column Name in Row</param>
	/// <returns></returns>
	public static T GetDbValue<T>(DataRow row, string columnName) => (T)Convert.ChangeType(row[columnName], typeof(T), CultureInfo.InvariantCulture);
}
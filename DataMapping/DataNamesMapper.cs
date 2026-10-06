using FCS.Utilities.DataMapping.Attributes;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace FCS.Utilities.DataMapping;

/// <summary>
/// Provides functionality to map data from a DataTable to a list of entities of type TEntity,
/// using the DataNamesAttribute to specify alternative column names for properties.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class DataNamesMapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity> where TEntity : class, new()
{
    /// <summary>
    /// Map A Single DataRow
    /// </summary>
    /// <param name="row">DataRow</param>
    public TEntity Map(DataRow row) => Map(row, new TEntity(), typeof(TEntity));

    /// <summary>
    /// Map A Single DataRow To TEntity
    /// </summary>
    /// <param name="row">Dataset Row</param>
    /// <param name="entity"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public TEntity Map(DataRow row, TEntity entity, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] System.Type type)
    {
        // Get Column Names
        List<string> columnNames = [.. row.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName)];

        // Get Properties
        foreach (PropertyInfo prop in (List<PropertyInfo>)[.. type.GetProperties().Where(x => x.GetCustomAttributes(typeof(DataNamesAttribute), true).Length!=0)])               // Process Each Property
        {
            // Assign Value To Property In New Entity
            PropertyMapHelper.Map(typeof(TEntity), row, prop, entity);
        }

        return entity;                                          // Return Entity
    }

    /// <summary>
    /// Convert A DataTable Contents To A List
    /// </summary>
    /// <param name="table"></param>
    public IEnumerable<TEntity> Map(DataTable table)
    {
        List<TEntity> entities = [];

        // Get Column Names
        List<string> columnNames = [.. table.Columns.Cast<DataColumn>().Select(x => x.ColumnName)];

        // Get Properties
        //List<PropertyInfo> properties = typeof(TEntity).GetProperties()
        //    .Where(x => x.GetCustomAttributes(typeof(DataNamesAttribute), true).Any())
        //    .ToList();

        List<PropertyInfo> properties = [.. typeof(TEntity).GetProperties()];

        foreach (DataRow row in table.Rows)                     // Process Each Row
        {
            TEntity entity = new();                       // Create New TEntity

            foreach (PropertyInfo prop in properties)           // Process Each Property
            {
                // Assign Value To Property In New Entity
                PropertyMapHelper.Map(typeof(TEntity), row, prop, entity);
            }

            entities.Add(entity);                               // Add Entity To List
        }

        return entities;                                        // Return Entities
    }
}
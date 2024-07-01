using System;
using System.Reflection;

namespace Dapper;

/// <summary>
/// Adapter for SQL syntax providers
/// </summary>
public interface ISqlAdapter
{
    /// <summary>
    /// Gets the name of the table that corresponds to the class type
    /// </summary>
    /// <param name="type">The type of the object</param>
    /// <returns>The name of the table</returns>
    string GetTableName(Type type);

    /// <summary>
    /// Gets the name of the column that corresponds to the property.
    /// </summary>
    /// <param name="property">The object property</param>
    /// <returns>The name of the column</returns>
    string GetColumnName(PropertyInfo property);

    /// <summary>
    /// Wraps the name of the column in quotes, brackets or other syntax necessary for the database.
    /// </summary>
    /// <param name="columnName">The name of the column</param>
    /// <returns>The column name wrapped in enclosed characters</returns>
    string EncapsulateColumn(string columnName);

    /// <summary>
    /// Gets the SQL used for selecting the last inserted identity column from a table.
    /// </summary>
    /// <param name="table">The name of the table</param>
    /// <param name="column">The name of the identity column</param>
    /// <returns>The SQL command for selecting inserted identities.</returns>
    string GetIdentitySql(string table, string column);
}

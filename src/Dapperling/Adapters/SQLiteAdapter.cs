using System;
using System.Reflection;

namespace Dapper;

/// <summary>
/// A SQL syntax provider for SQLite
/// </summary>
public class SQLiteAdapter : ISqlAdapter
{
    /// <inheritdoc />
    public virtual string GetTableName(Type type)
    {
        return type.GetDefaultTableName();
    }

    /// <inheritdoc />
    public virtual string GetColumnName(PropertyInfo property)
    {
        return property.Name;
    }

    /// <inheritdoc />
    public virtual string EncapsulateColumn(string columnName)
    {
        return $"\"{columnName}\"";
    }
}

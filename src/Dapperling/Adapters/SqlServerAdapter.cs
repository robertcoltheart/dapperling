using System;
using System.Reflection;

namespace Dapper;

/// <summary>
/// A SQL syntax provider for SQL Server
/// </summary>
public class SqlServerAdapter : ISqlAdapter
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
        return $"[{columnName}]";
    }

    /// <inheritdoc />
    public string GetIdentitySql(string table, string column)
    {
        return "select SCOPE_IDENTITY() id";
    }
}

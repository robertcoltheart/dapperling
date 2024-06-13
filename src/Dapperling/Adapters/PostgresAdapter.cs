using System;
using System.Linq;
using System.Reflection;

namespace Dapper;

/// <summary>
/// A SQL syntax provider for PostgreSQL
/// </summary>
/// <remarks>
/// By default, the PostgreSQL provider uses `lower_snake_case_plural` for table names, and `lower_snake_case` for columns.
/// </remarks>
public class PostgresAdapter : ISqlAdapter
{
    /// <inheritdoc />
    public virtual string GetTableName(Type type)
    {
        var name = type.GetDefaultTableName();

        return GetKebabCase(name);
    }

    /// <inheritdoc />
    public virtual string GetColumnName(PropertyInfo property)
    {
        return GetKebabCase(property.Name);
    }

    /// <inheritdoc />
    public virtual string EncapsulateColumn(string columnName)
    {
        return $"\"{columnName}\"";
    }

    /// <inheritdoc />
    public string GetIdentitySql(string table, string column)
    {
        return "SELECT LASTVAL() AS id";
    }

    private string GetKebabCase(string value)
    {
        var values = value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString());

        return string.Concat(values).ToLower();
    }
}

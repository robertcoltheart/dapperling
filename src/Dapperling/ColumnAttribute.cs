using System;

namespace Dapper;

/// <summary>
/// Attribute for explicitly setting a column name
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="ColumnAttribute"/>.
    /// </summary>
    /// <param name="name">Name of the column</param>
    public ColumnAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the column
    /// </summary>
    public string Name { get; }
}

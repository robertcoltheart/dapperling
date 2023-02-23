using System;

namespace Dapper;

/// <summary>
/// Attribute for explicitly setting a table name
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="TableAttribute"/>.
    /// </summary>
    /// <param name="name">Name of the table</param>
    public TableAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the table name
    /// </summary>
    public string Name { get; }
}

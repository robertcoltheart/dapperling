using System;

namespace Dapper;

/// <summary>
/// Attribute to exclude columns from insertion or updating
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreAttribute : Attribute
{
}

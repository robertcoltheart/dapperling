﻿using System;

namespace Dapper;

/// <summary>
/// Attribute for specifying a column key that is generated by the database
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute
{
}

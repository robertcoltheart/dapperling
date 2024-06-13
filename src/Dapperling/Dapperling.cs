using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper;

/// <summary>
/// Extension methods for executing basic queries using an <see cref="IDbConnection"/>.
/// </summary>
public static class Dapperling
{
    internal static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();

    internal static readonly Dictionary<string, ISqlAdapter> Adapters = new()
    {
        {"npgsqlconnection", new PostgresAdapter()},
        {"sqlconnection", new SqlServerAdapter()},
        {"sqliteconnection", new SQLiteAdapter()},
        {"mysqlconnection", new MySqlAdapter()},
        {"fbconnection", new FirebaseAdapter()}
    };

    /// <summary>
    /// Gets the database type name, given an <see cref="IDbConnection"/>
    /// </summary>
    public static Func<IDbConnection, string>? GetDatabaseType;

    /// <summary>
    /// Register a SQL syntax provider using a specific name, or override the provider of an existing default provider.
    /// </summary>
    /// <param name="name">The name of the adapter</param>
    /// <param name="adapter">The SQL syntax provider</param>
    public static void RegisterAdapter(string name, ISqlAdapter adapter)
    {
        Adapters[name] = adapter;
    }
}

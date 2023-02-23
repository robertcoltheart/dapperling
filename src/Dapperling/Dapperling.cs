using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper;

/// <summary>
/// Extension methods for executing basic queries using an <see cref="IDbConnection"/>.
/// </summary>
public static class Dapperling
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> KeyProperties = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ExplicitKeyProperties = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo> KeyProperty = new();

    private static readonly ConcurrentDictionary<Type, string> TableNames = new();

    private static readonly ConcurrentDictionary<PropertyInfo, string> ColumnNames = new();

    private static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();

    private static readonly Dictionary<string, ISqlAdapter> Adapters = new()
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

    /// <summary>
    /// Gets a row using its key
    /// </summary>
    /// <typeparam name="T">The type of the object to get</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="id">The key value to query</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The object from the table, or null if no row was found</returns>
    public static T? Get<T>(this IDbConnection connection, object id, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetSelectSql<T>(connection);

        return connection.Query<T>(sql, new {id}, transaction, commandTimeout: commandTimeout).FirstOrDefault();
    }

    /// <summary>
    /// Gets a row using its key for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to get</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="id">The key value to query</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The object from the table, or null if no row was found</returns>
    public static async Task<T?> GetAsync<T>(this IDbConnection connection, object id, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetSelectSql<T>(connection);

        var value = await connection.QueryAsync<T>(sql, new {id}, transaction, commandTimeout: commandTimeout).ConfigureAwait(false);

        return value.FirstOrDefault();
    }

    /// <summary>
    /// Get all rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to get</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>All rows for the specified type</returns>
    public static IEnumerable<T> GetAll<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetSelectAllSql<T>(connection);

        return connection.Query<T>(sql, null, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Get all rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to get</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>All rows for the specified type</returns>
    public static async Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetSelectAllSql<T>(connection);

        return await connection.QueryAsync<T>(sql, null, transaction, commandTimeout: commandTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to insert</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to insert</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static int Insert<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetInsertSql<T>(connection);

        return connection.Execute(sql, entity, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Inserts a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to insert</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to insert</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static async Task<int> InsertAsync<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetInsertSql<T>(connection);

        return await connection.ExecuteAsync(sql, entity, transaction, commandTimeout: commandTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to update</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to update</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static int Update<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetUpdateSql<T>(connection);

        return connection.Execute(sql, entity, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Updates a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to update</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to update</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static async Task<int> UpdateAsync<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetUpdateSql<T>(connection);

        return await connection.ExecuteAsync(sql, entity, transaction, commandTimeout: commandTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to delete</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to delete</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static int Delete<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetDeleteSql<T>(connection);

        return connection.Execute(sql, entity, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Deletes a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to delete</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to delete</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static async Task<int> DeleteAsync<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetDeleteSql<T>(connection);

        return await connection.ExecuteAsync(sql, entity, transaction, commandTimeout: commandTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes all rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to delete</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static int DeleteAll<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetDeleteAllSql<T>(connection);

        return connection.Execute(sql, null, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Deletes all rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to delete</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static async Task<int> DeleteAllAsync<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetDeleteAllSql<T>(connection);

        return await connection.ExecuteAsync(sql, null, transaction, commandTimeout: commandTimeout).ConfigureAwait(false);
    }

    private static string GetSelectSql<T>(IDbConnection connection)
    {
        var property = GetKeyProperty(typeof(T));
        var columnName = GetColumnName(connection, property);
        var tableName = GetTableName(connection, typeof(T));

        return $"select * from {tableName} where {columnName} = @id;";
    }

    private static string GetSelectAllSql<T>(IDbConnection connection)
    {
        var tableName = GetTableName(connection, typeof(T));

        return $"select * from {tableName}";
    }

    private static string GetInsertSql<T>(IDbConnection connection)
    {
        var type = typeof(T).GetCollectionElementType() ?? typeof(T);

        var properties = GetProperties(type);
        var keyProperties = GetKeyProperties(type);
        var insertableProperties = properties.Except(keyProperties);

        var adapter = GetAdapter(connection);

        var columns = new StringBuilder();
        var parameters = new StringBuilder();

        foreach (var property in insertableProperties)
        {
            if (columns.Length > 0)
            {
                columns.Append(", ");
                parameters.Append(", ");
            }

            var columnName = GetColumnName(connection, property);

            columns.Append(adapter.EncapsulateColumn(columnName));
            parameters.Append($"@{property.Name}");
        }

        var tableName = GetTableName(connection, type);

        return $"insert into {tableName} ({columns}) values ({parameters})";
    }

    private static string GetUpdateSql<T>(IDbConnection connection)
    {
        var type = typeof(T).GetCollectionElementType() ?? typeof(T);

        var keyProperties = GetRequiredKeyProperties(type);
        var nonKeyProperties = GetProperties(type).Except(keyProperties);

        var tableName = GetTableName(connection, type);
        var columns = JoinColumnsQuery(connection, nonKeyProperties, ", ");
        var where = JoinColumnsQuery(connection, keyProperties, " and ");

        return $"update {tableName} set {columns} where {where}";
    }

    private static string GetDeleteSql<T>(IDbConnection connection)
    {
        var type = typeof(T).GetCollectionElementType() ?? typeof(T);

        var keyProperties = GetRequiredKeyProperties(type);

        var tableName = GetTableName(connection, type);
        var where = JoinColumnsQuery(connection, keyProperties, " and ");

        return $"delete from {tableName} where {where}";
    }

    private static string GetDeleteAllSql<T>(IDbConnection connection)
    {
        var tableName = GetTableName(connection, typeof(T));

        return $"delete from {tableName}";
    }

    private static string JoinColumnsQuery(IDbConnection connection, IEnumerable<PropertyInfo> properties, string joinValue)
    {
        var adapter = GetAdapter(connection);

        var value = new StringBuilder();

        foreach (var property in properties)
        {
            if (value.Length > 0)
            {
                value.Append(joinValue);
            }

            var columnName = GetColumnName(connection, property);

            value.Append($"{adapter.EncapsulateColumn(columnName)} = @{property.Name}");
        }

        return value.ToString();
    }

    private static ISqlAdapter GetAdapter(IDbConnection connection)
    {
        var databaseName = GetDatabaseType != null
            ? GetDatabaseType(connection)
            : connection.GetType().Name.ToLower();

        return Adapters.TryGetValue(databaseName, out var value)
            ? value
            : DefaultAdapter;
    }

    private static string GetTableName(IDbConnection connection, Type type)
    {
        return TableNames.GetOrAdd(type, _ =>
        {
            var attribute = type.GetCustomAttribute<TableAttribute>();

            return attribute != null
                ? attribute.Name
                : GetAdapter(connection).GetTableName(type);
        });
    }

    private static string GetColumnName(IDbConnection connection, PropertyInfo property)
    {
        return ColumnNames.GetOrAdd(property, x =>
        {
            var attribute = x.GetCustomAttribute<ColumnAttribute>();

            return attribute != null
                ? attribute.Name
                : GetAdapter(connection).GetColumnName(x);
        });
    }

    private static PropertyInfo[] GetRequiredKeyProperties(Type type)
    {
        var properties = GetKeyProperties(type).Union(GetExplicitKeyProperties(type)).ToArray();

        if (!properties.Any())
        {
            throw new InvalidOperationException($"'{type}' must have a single [Key] or [ExplicitKey], or a property named Id");
        }

        return properties;
    }

    private static PropertyInfo GetKeyProperty(Type type)
    {
        return KeyProperty.GetOrAdd(type, _ =>
        {
            var keys = GetKeyProperties(type);
            var explicitKeys = GetExplicitKeyProperties(type);

            var count = keys.Length + explicitKeys.Length;

            if (count != 1)
            {
                throw new InvalidOperationException($"'{type}' must have a single [Key] or [ExplicitKey], or a property named Id");
            }

            return keys.Length > 0
                ? keys[0]
                : explicitKeys[0];
        });
    }

    private static PropertyInfo[] GetExplicitKeyProperties(Type type)
    {
        return ExplicitKeyProperties.GetOrAdd(type, _ =>
            GetProperties(type)
                .Where(x => x.GetCustomAttribute<ExplicitKeyAttribute>() != null)
                .ToArray());
    }

    private static PropertyInfo[] GetKeyProperties(Type type)
    {
        return KeyProperties.GetOrAdd(type, _ =>
        {
            var properties = GetProperties(type);
            var keyProperties = properties
                .Where(x => x.GetCustomAttribute<KeyAttribute>() != null)
                .ToList();

            if (!keyProperties.Any())
            {
                var id = properties.FirstOrDefault(x => x.Name.Equals("id", StringComparison.OrdinalIgnoreCase));

                if (id != null && id.GetCustomAttribute<ExplicitKeyAttribute>() == null)
                {
                    keyProperties.Add(id);
                }
            }

            return keyProperties.ToArray();
        });
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        return Properties.GetOrAdd(type, _ =>
            type.GetProperties()
                .Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null)
                .ToArray());
    }
}

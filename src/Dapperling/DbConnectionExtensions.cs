﻿using System;
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
public static class DbConnectionExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> KeyProperties = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ExplicitKeyProperties = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo> KeyProperty = new();

    private static readonly ConcurrentDictionary<(Type, string), string> TableNames = new();

    private static readonly ConcurrentDictionary<(PropertyInfo, string), string> ColumnNames = new();

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

        return connection.Query<T>(sql, new { id }, transaction, commandTimeout: commandTimeout).FirstOrDefault();
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

        var value = await connection.QueryAsync<T>(sql, new { id }, transaction, commandTimeout).ConfigureAwait(false);

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
    /// Get all rows for a specified type given the specified where clauses
    /// </summary>
    /// <typeparam name="T">The type of the object to get</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="whereClause">The object containing where clauses</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>All rows for the specified type</returns>
    public static IEnumerable<T> GetAll<T>(this IDbConnection connection, object whereClause, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetSelectAllSql<T>(connection);
        var where = GetWhereClause<T>(connection, whereClause);

        return connection.Query<T>($"{sql} where {where}", whereClause, transaction, commandTimeout: commandTimeout);
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

        return await connection.QueryAsync<T>(sql, null, transaction, commandTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Get all rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to get</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="whereClause">The object containing where clauses</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>All rows for the specified type</returns>
    public static async Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, object whereClause, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetSelectAllSql<T>(connection);
        var where = GetWhereClause<T>(connection, whereClause);

        return await connection.QueryAsync<T>($"{sql} where {where}", whereClause, transaction, commandTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to insert</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to insert</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The id of the inserted row or the number of rows inserted if a collection is inserted</returns>
    public static long Insert<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetInsertSql<T>(connection);

        var keyProperties = GetKeyProperties(typeof(T));

        if (keyProperties.Any())
        {
            var result = connection.QueryFirst<long>(sql, entity, transaction, commandTimeout);

            var property = keyProperties.First();
            var keyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            
            property.SetValue(entity, Convert.ChangeType(result, keyType));

            return result;
        }

        return connection.Execute(sql, entity, transaction, commandTimeout);
    }

    /// <summary>
    /// Inserts a row or a collection of rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to insert</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="entity">The object data to insert</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The id of the inserted row or the number of rows inserted if a collection is inserted</returns>
    public static async Task<long> InsertAsync<T>(this IDbConnection connection, T entity, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetInsertSql<T>(connection);

        var keyProperties = GetKeyProperties(typeof(T));

        if (keyProperties.Any())
        {
            var result = await connection.QueryFirstAsync<long>(sql, entity, transaction, commandTimeout).ConfigureAwait(false);

            var property = keyProperties.First();
            var keyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            property.SetValue(entity, Convert.ChangeType(result, keyType));

            return result;
        }

        return await connection.ExecuteAsync(sql, entity, transaction, commandTimeout).ConfigureAwait(false);
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

        return connection.Execute(sql, entity, transaction, commandTimeout);
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

        return await connection.ExecuteAsync(sql, entity, transaction, commandTimeout).ConfigureAwait(false);
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

        return connection.Execute(sql, entity, transaction, commandTimeout);
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

        return await connection.ExecuteAsync(sql, entity, transaction, commandTimeout).ConfigureAwait(false);
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

        return connection.Execute(sql, null, transaction, commandTimeout);
    }

    /// <summary>
    /// Deletes all rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to delete</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="whereClause">The object containing where clauses</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static int DeleteAll<T>(this IDbConnection connection, object whereClause, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetDeleteAllSql<T>(connection);
        var where = GetWhereClause<T>(connection, whereClause);

        return connection.Execute($"{sql} where {where}", whereClause, transaction, commandTimeout);
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

        return await connection.ExecuteAsync(sql, null, transaction, commandTimeout).ConfigureAwait(false);
    }


    /// <summary>
    /// Deletes all rows for a specified type
    /// </summary>
    /// <typeparam name="T">The type of the object to delete</typeparam>
    /// <param name="connection">The database connection</param>
    /// <param name="whereClause">The object containing where clauses</param>
    /// <param name="transaction">The transaction to run the query in</param>
    /// <param name="commandTimeout">The timeout in seconds</param>
    /// <returns>The number of affected rows</returns>
    public static async Task<int> DeleteAllAsync<T>(this IDbConnection connection, object whereClause, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class
    {
        var sql = GetDeleteAllSql<T>(connection);
        var where = GetWhereClause<T>(connection, whereClause);

        return await connection.ExecuteAsync($"{sql} where {where}", whereClause, transaction, commandTimeout).ConfigureAwait(false);
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
        var identityQuery = keyProperties.Any() && type == typeof(T)
            ? $";{adapter.GetIdentitySql(tableName, GetColumnName(connection, keyProperties.First()))}"
            : string.Empty;

        return $"insert into {tableName} ({columns}) values ({parameters}){identityQuery}";
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

    private static string? GetWhereClause<T>(IDbConnection connection, object? whereClause)
    {
        if (whereClause == null)
        {
            return null;
        }

        var adapter = GetAdapter(connection);

        var whereProperties = whereClause.GetType().GetProperties();
        var entityProperties = GetProperties(typeof(T));

        var clause = new StringBuilder();

        for (var i = 0; i < whereProperties.Length; i++)
        {
            var property = whereProperties[i];

            if (!property.CanRead)
            {
                continue;
            }

            var entityProperty = entityProperties.FirstOrDefault(x => x.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

            var columnName = entityProperty != null
                ? GetColumnName(connection, entityProperty)
                : property.Name;

            var isNull = property.GetValue(whereClause, null) is null or DBNull;

            if (isNull)
            {
                clause.Append($"{adapter.EncapsulateColumn(columnName)} is null");
            }
            else
            {
                clause.Append($"{adapter.EncapsulateColumn(columnName)} = @{property.Name}");
            }

            if (i < whereProperties.Length - 1)
            {
                clause.Append(" and ");
            }
        }

        return clause.ToString();
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

    private static string GetDatabaseName(IDbConnection connection)
    {
        return Dapperling.GetDatabaseType != null
            ? Dapperling.GetDatabaseType(connection)
            : connection.GetType().Name.ToLower();
    }

    private static ISqlAdapter GetAdapter(IDbConnection connection)
    {
        var databaseName = GetDatabaseName(connection);

        return Dapperling.Adapters.TryGetValue(databaseName, out var value)
            ? value
            : Dapperling.DefaultAdapter;
    }

    private static string GetTableName(IDbConnection connection, Type type)
    {
        var databaseName = GetDatabaseName(connection);

        return TableNames.GetOrAdd((type, databaseName), _ =>
        {
            var attribute = type.GetCustomAttribute<TableAttribute>();

            return attribute != null
                ? attribute.Name
                : GetAdapter(connection).GetTableName(type);
        });
    }

    private static string GetColumnName(IDbConnection connection, PropertyInfo property)
    {
        var databaseName = GetDatabaseName(connection);

        return ColumnNames.GetOrAdd((property, databaseName), _ =>
        {
            var attribute = property.GetCustomAttribute<ColumnAttribute>();

            return attribute != null
                ? attribute.Name
                : GetAdapter(connection).GetColumnName(property);
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

using Apps72.Dev.Data.DbMocker;
using System.Data;

namespace Dapper.Tests;

public abstract class MockConnectionTests
{
    protected IDbConnection GetScalarConnection(string expectedSql)
    {
        var connection = new MockDbConnection();
        connection.Mocks
            .HasValidSqlServerCommandText()
            .When(x => x.CommandText.StartsWith(expectedSql))
            .ReturnsScalar(1);

        return connection;
    }

    protected IDbConnection GetRowConnection<T>(string expectedSql, T row)
    {
        var connection = new MockDbConnection();
        connection.Mocks
            .HasValidSqlServerCommandText()
            .When(x => x.CommandText == expectedSql)
            .ReturnsRow(row);

        return connection;
    }
}

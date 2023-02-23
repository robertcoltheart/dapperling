using Dapper;
using Dapperling.Tests.Models;
using Xunit;

namespace Dapperling.Tests.Adapters;

public class PostgresAdapterTests
{
    [Fact]
    public void ConvertsTableNameCorrectly()
    {
        var adapter = new PostgresAdapter();

        var name = adapter.GetTableName(typeof(PascalCase));

        Assert.Equal("pascal_cases", name);
    }
}

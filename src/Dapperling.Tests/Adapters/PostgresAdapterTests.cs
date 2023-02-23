using Dapper.Tests.Models;
using Xunit;

namespace Dapper.Tests.Adapters;

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

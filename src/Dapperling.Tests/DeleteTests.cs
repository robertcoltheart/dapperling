using System.Threading.Tasks;
using Dapper.Tests.Models;
using Xunit;

namespace Dapper.Tests;

public class DeleteTests : MockConnectionTests
{
    [Fact]
    public void CanDeleteGenericType()
    {
        var result = GetScalarConnection("delete from Articles where [Id] = @Id")
            .Delete(new GenericType<Article> { Id = "1", Name = "Type" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanDelete()
    {
        var result = GetScalarConnection("delete from Articles where [Id] = @Id")
            .Delete(new Article { Id = 1, Title = "title" });

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task CanDeleteAsync()
    {
        var result = await GetScalarConnection("delete from Articles where [Id] = @Id")
            .DeleteAsync(new Article { Id = 1, Title = "title" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanDeleteCollection()
    {
        var items = new[]
        {
            new Article {Id = 1, Title = "title"},
            new Article {Id = 2, Title = "title2"}
        };

        var result = GetScalarConnection("delete from Articles where [Id] = @Id")
            .Delete(items);

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task CanDeleteCollectionAsync()
    {
        var items = new[]
        {
            new Article {Id = 1, Title = "title"},
            new Article {Id = 2, Title = "title2"}
        };

        var result = await GetScalarConnection("delete from Articles where [Id] = @Id")
            .DeleteAsync(items);

        Assert.Equal(2, result);
    }

    [Fact]
    public void CanDeleteAll()
    {
        var result = GetScalarConnection("delete from Articles")
            .DeleteAll<Article>();

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanDeleteAllWithWhereClause()
    {
        var result = GetScalarConnection("delete from Articles where [Title] = @title")
            .DeleteAll<Article>(new { title = "Tale of Three Cities" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanDeleteAllWithWhereClauseWithNamedColumn()
    {
        var result = GetScalarConnection("delete from table_name where [column_name] = @name")
            .DeleteAll<TableWithExplicitColumn>(new { name = "Tale of Three Cities" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanDeleteAllWithWhereClauseWithUnknownColumn()
    {
        var result = GetScalarConnection("delete from table_name where [column_name] = @column_name")
            .DeleteAll<TableWithExplicitColumn>(new { column_name = "Tale of Three Cities" });

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task CanDeleteAllAsync()
    {
        var result = await GetScalarConnection("delete from Articles")
            .DeleteAllAsync<Article>();

        Assert.Equal(1, result);
    }
}

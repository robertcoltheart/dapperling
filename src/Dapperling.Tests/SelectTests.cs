using System.Linq;
using Dapper.Tests.Models;
using Xunit;

namespace Dapper.Tests;

public class SelectTests : MockConnectionTests
{
    [Fact]
    public void CanDeleteExplicitKey()
    {
        var result = GetScalarConnection("delete from explicit_article where [Key] = @Key")
            .Delete(new ArticleWithExplicitKey { Key = "1", Name = "Type" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanGetAllWithExplicitKey()
    {
        var result = GetRowConnection("select * from explicit_article", new {Key = "123", Name = "name"})
            .GetAll<ArticleWithExplicitKey>().ToArray();

        Assert.Single(result);
    }

    [Fact]
    public void CanGet()
    {
        var result = GetRowConnection("select * from Articles where Id = @id;", new { Id = 1, Title = "title" })
            .Get<Article>(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("title", result.Title);
    }

    [Fact]
    public void CanGetAll()
    {
        var result = GetRowConnection("select * from Articles", new {Id = 1, Title = "title"})
            .GetAll<Article>().ToArray();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("title", result[0].Title);
    }
}

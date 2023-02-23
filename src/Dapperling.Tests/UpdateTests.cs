using Dapper.Tests.Models;
using Xunit;

namespace Dapper.Tests;

public class UpdateTests : MockConnectionTests
{
    [Fact]
    public void CanUpdateGenericType()
    {
        var result = GetScalarConnection("update Articles set [Title] = @Title, [Description] = @Description where [Id] = @Id")
            .Update(new GenericType<Article> { Id = "1", Name = "Type" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanUpdateExplicitKey()
    {
        var result = GetScalarConnection("update explicit_article set [Name] = @Name where [Key] = @Key")
            .Update(new ArticleWithExplicitKey { Key = "1", Name = "Type" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanUpdate()
    {
        var result = GetScalarConnection("update Articles set [Title] = @Title, [Description] = @Description where [Id] = @Id")
            .Update(new Article { Id = 1, Title = "title" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanUpdateCollection()
    {
        var items = new[]
        {
            new Article {Id = 1, Title = "title"},
            new Article {Id = 2, Title = "title2"}
        };

        var result = GetScalarConnection("update Articles set [Title] = @Title, [Description] = @Description where [Id] = @Id")
            .Update(items);

        Assert.Equal(2, result);
    }

    [Fact]
    public void IgnoresPropertyOnUpdate()
    {
        var result = GetScalarConnection("update UserWithIgnoreds set [Name] = @Name where [Id] = @Id")
            .Update(new UserWithIgnored { Id = 1, Name = "name", Email = "email" });

        Assert.Equal(1, result);
    }
}

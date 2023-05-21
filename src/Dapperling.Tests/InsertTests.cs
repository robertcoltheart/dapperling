using System;
using System.Threading.Tasks;
using Dapper.Tests.Models;
using Xunit;

namespace Dapper.Tests;

public class InsertTests : MockConnectionTests
{
    [Fact]
    public void CanInsertGenericType()
    {
        var result = GetScalarConnection("insert into Articles ([Title], [Description]) values (@Title, @Description)")
            .Insert(new GenericType<Article> { Id = "1", Name = "Type" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanInsertExplicitKey()
    {
        var result = GetScalarConnection("insert into explicit_article ([Key], [Name]) values (@Key, @Name)")
            .Insert(new ArticleWithExplicitKey { Key = "1", Name = "Type" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanInsert()
    {
        var result = GetScalarConnection("insert into Articles ([Title], [Description]) values (@Title, @Description)")
            .Insert(new Article { Id = 1, Title = "title" });

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task CanInsertAsync()
    {
        var result = await GetScalarConnection("insert into Articles ([Title], [Description]) values (@Title, @Description)")
            .InsertAsync(new Article { Id = 1, Title = "title" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanInsertCollection()
    {
        var items = new[]
        {
            new Article {Id = 1, Title = "title"},
            new Article {Id = 2, Title = "title2"}
        };

        var result = GetScalarConnection("insert into Articles ([Title], [Description]) values (@Title, @Description)")
            .Insert(items);

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task CanInsertCollectionAsync()
    {
        var items = new[]
        {
            new Article {Id = 1, Title = "title"},
            new Article {Id = 2, Title = "title2"}
        };

        var result = await GetScalarConnection("insert into Articles ([Title], [Description]) values (@Title, @Description)")
            .InsertAsync(items);

        Assert.Equal(2, result);
    }

    [Fact]
    public void IgnoresPropertyOnInsert()
    {
        var result = GetScalarConnection("insert into UserWithIgnoreds ([Name]) values (@Name)")
            .Insert(new UserWithIgnored { Id = 1, Name = "name", Email = "email" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanInsertIntoCorrectTableUsingPostgres()
    {
        var result = GetScalarConnection("insert into Articles ([Title], [Description]) values (@Title, @Description)")
            .Insert(new Article { Id = 1, Title = "title" });

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanInsertWithNullableKey()
    {
        var result = GetScalarConnection("insert into ModelWithNullables ([DateTime]) values (@DateTime)")
            .Insert(new ModelWithNullable { Id = 1, DateTime = DateTime.Now });

        Assert.Equal(1, result);
    }
}

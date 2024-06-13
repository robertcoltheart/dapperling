using System;
using System.Linq;
using Npgsql;
using Xunit;

namespace Dapper.Tests;

public class IntegrationTests : IDisposable
{
    private const string ConnectionString = "Host=localhost;Database=postgres;User ID=postgres;Password=postgres";

    public IntegrationTests()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();

        connection.Execute("create table blog_metadatas (id int generated always as identity, key text, tag_value text)");
    }

    [IntegrationFact]
    public void CanInsert()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        var position = new BlogMetadata
        {
            Key = "title",
            TagValue = "value"
        };

        var result = connection.Insert(position);

        Assert.Equal(position.Id, result);
    }

    [IntegrationFact]
    public void CanInsertCollection()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        var position = new BlogMetadata
        {
            Key = "title",
            TagValue = "value"
        };

        var result = connection.Insert(new[] {position, position});

        Assert.Equal(2, result);
    }

    [IntegrationFact]
    public void CanUpdate()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        var position = new BlogMetadata
        {
            Key = "title",
            TagValue = "value"
        };

        connection.Insert(position);
        var item = connection.GetAll<BlogMetadata>().First();

        position.Id = item.Id;
        position.TagValue = "unique";

        var result = connection.Update(position);
        var updated = connection.Get<BlogMetadata>(item.Id);

        Assert.Equal(1, result);
        Assert.Equal("unique", updated!.TagValue);
    }

    [IntegrationFact]
    public void CanDelete()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        var position = new BlogMetadata
        {
            Id = 1,
            Key = "title",
            TagValue = "value"
        };

        connection.Insert(position);
        var item = connection.GetAll<BlogMetadata>().First();

        position.Id = item.Id;

        var result = connection.Delete(position);

        Assert.Equal(1, result);
    }

    [IntegrationFact]
    public void CanDeleteAll()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        var position = new BlogMetadata
        {
            Id = 1,
            Key = "title",
            TagValue = "value"
        };

        connection.Insert(position);
        var count = connection.GetAll<BlogMetadata>().Count();

        var result = connection.DeleteAll<BlogMetadata>();

        Assert.Equal(count, result);
    }

    [IntegrationFact]
    public void CanSelect()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        var position = new BlogMetadata
        {
            Id = 1,
            Key = "title",
            TagValue = "value"
        };

        connection.Insert(position);
        var item = connection.GetAll<BlogMetadata>().First();

        var result = connection.Get<BlogMetadata>(item.Id);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("title", result.Key);
        Assert.Equal("value", result.TagValue);
    }

    public void Dispose()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();

        connection.Execute("drop table blog_metadatas");
    }

    private class BlogMetadata
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string TagValue { get; set; }
    }
}

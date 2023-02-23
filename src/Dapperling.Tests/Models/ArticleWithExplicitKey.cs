namespace Dapper.Tests.Models;

[Table("explicit_article")]
public class ArticleWithExplicitKey
{
    [ExplicitKey]
    public string Key { get; set; }

    public string Name { get; set; }
}

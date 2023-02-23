namespace Dapper.Tests.Models;

public class GenericType<T>
{
    [ExplicitKey]
    public string Id { get; set; }

    public string Name { get; set; }
}

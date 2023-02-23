using Dapper;

namespace Dapperling.Tests.Models;

public class UserWithIgnored
{
    public short Id { get; set; }

    public string Name { get; set; }

    [Ignore]
    public string Email { get; set; }
}

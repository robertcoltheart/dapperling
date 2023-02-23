using System;

namespace Dapper.Tests.Models;

public class ModelWithNullable
{
    public int? Id { get; set; }

    public DateTime? DateTime { get; set; }
}

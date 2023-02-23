using Dapper;

namespace Dapperling.Tests.Models;

[Table("table_name")]
public class TableWithExplicitColumn
{
    public string Id { get; set; }

    [Column("column_name")]
    public string Name { get; set; }
}

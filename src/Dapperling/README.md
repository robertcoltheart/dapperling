# Dapperling 

[![NuGet](https://img.shields.io/nuget/v/Inject?style=for-the-badge)](https://www.nuget.org/packages/Dapperling) [![License](https://img.shields.io/github/license/robertcoltheart/dapperling?style=for-the-badge)](https://github.com/robertcoltheart/dapperling/blob/master/LICENSE)

A CRUD helper for Dapper for quickly selecting, inserting, updating or deleting data.

Provides syntax support for the following databases:

 - SQL Server
 - MySQL
 - PostgreSQL
 - SQLite
 - Firebase

## Usage
Install the package from NuGet with `dotnet add package Dapperling`.

The below features are supported by Dapperling:

| Method | Description |
| -- | -- |
| `Get` | Get an object by id |
| `GetAll` | Get all objects of a specific type |
| `Insert` | Insert an object or collection of objects |
| `Update` | Update an object by id, or a collection of objects by their ids |
| `Delete` | Delete an object by id, or a collection of objects by their ids |
| `DeleteAll` | Delete all objects of a specific type |

## Querying and updating data
Use the below methods to quickly access data in your tables.

### `Get` objects
Get an object by its id:

```c#
var article = connection.Get<Article>(1);
```

Get all objects of a specific type:

```c#
var articles = connection.GetAll<Article>();
```

### `Insert` objects
Insert an object:

```c#
connection.Insert(new Article { Title = "Name" });
```

Insert a collection of objects:

```c#
connection.Insert(new[] { new Article { Title = "Name" } });
```

### `Update` objects
Update an object by its id:

```c#
connection.Update(new Article { Id = 1, Title = "Name" });
```

Update a collection of objects by their ids:

```c#
connection.Update(new[] { new Article { Id = 1, Title = "Name" } });
```

### `Delete` objects
Delete an object by its id:

```c#
connection.Delete(new Article { Id = 1 });
```

Delete a collection of objects by their ids:
```c#
connection.Delete(new[] { new Article { Id = 1 } });
```

Delete all objects of a specific type:

```c#
connection.DeleteAll<Article>();
```

## Customizing queries
You can use several attributes to control the names of tables and columns, or control how the queries are generated.

### `[Table]` attribute
Use the `[Table]` attribute to control the name of the table to use. By default, Dapperling pluralizes the name of your class as the table name, or in the case of Postgres,
uses `plural_snake_case` as the table name.

```c#
[Table("articles")]
public class Article
{
    public int Id { get; set; }
}
```

### `[Column]` attribute
Use the `[Column]` attribute to control the name of the column for each property. By default, the property name is used, or in the case of Postgres, uses `snake_case`.

```c#
public class Article
{
    public int Id { get; set; }

    [Column("column_name")]
    public string Name { get; set; }
}
```

### `[Key]` attribute
Use the `[Key]` attribute to specify the identity column for your table. By default, a property named `Id` is used.

```c#
public class Article
{
    [Key]
    public int ArticleId { get; set; }

    public string Name { get; set; }
}
```

### `[ExplicitKey]` attribute
If your primary key is not an identity column, use `[ExplicitKey]` to pass in the value instead of letting the database generate it.

```c#
public class Article
{
    [ExplicitKey]
    public Guid ArticleId { get; set; }

    public string Name { get; set; }
}
```

### `[Ignore]` attribute
Use the `[Ignore]` attribute on properties to exclude them from insertion or updating.

```c#
public class Article
{
    public int Id { get; set; }

    [Ignore]
    public string Name { get; set; }
}
```

## Extending Dapperling
By default, Dapplerling uses the lowercase name of the `DbConnection` used to determine the query syntax.
To help Dapperling determine the type of connection you are using, you can use the below:

```c#
Dapperling.GetDatabaseType = connection => connection.GetType().Name;
```

You can also register new query syntax providers, as below:

```c#
Dapperling.RegisterAdapter("myconnection", new MyAdapter());

Dapperling.GetDatabaseType = connection => "myconnection";
```

If your connection type isn't recognised or you don't register an adapter, by default the SQL Server syntax is used.

## Contributing
Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on how to contribute to this project.

## Acknowledgements
Inspired by [Dapper.Contrib](https://github.com/DapperLib/Dapper.Contrib) and the excellent [Dapper.SimpleCRUD](https://github.com/ericdc1/Dapper.SimpleCRUD) :heart:

## License
Inject is released under the [MIT License](LICENSE)

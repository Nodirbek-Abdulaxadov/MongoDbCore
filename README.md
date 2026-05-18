# MongoDbCore

[![NuGet](https://img.shields.io/nuget/v/MongoDbCore.svg)](https://www.nuget.org/packages/MongoDbCore/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A lightweight micro-ORM for MongoDB on .NET 8 and .NET 9. Built on top of
`MongoDB.Driver` 2.x with an EF-Core-flavoured surface: a `MongoDbContext`
that exposes typed `Collection<T>` properties, conventional collection naming,
`Include`/`IncludeRef` eager loading, optional auditing, and an in-memory
`SelfCachedCollection<T>` for read-heavy workloads.

## Install

```bash
dotnet add package MongoDbCore
```

## Quick start

```jsonc
// appsettings.json
"MongoDB": {
  "Connection": "mongodb://localhost:27017",
  "Database": "myapp",
  "MaxConnectionPoolSize": 100,
  "EnableAuditing": false
}
```

```csharp
// Program.cs
builder.Services.AddMongoDbContext<AppDbContext>(
    builder.Configuration.GetSection("MongoDB").Get<MongoDbCoreOptions>()!);
```

```csharp
// AppDbContext.cs
public class AppDbContext(MongoDbCoreOptions options) : MongoDbContext(options)
{
    public Collection<Todo> Todos { get; set; } = null!;
    public SelfCachedCollection<Country> Countries { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        // seed indexes / initial data here
        return Task.CompletedTask;
    }
}
```

```csharp
// Todo.cs
public class Todo : BaseEntity
{
    public string Task { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
```

```csharp
// usage
public class TodoService(AppDbContext db)
{
    public Task<List<Todo>> ListAsync(CancellationToken ct) =>
        db.Todos.ToListAsync(ct);

    public Task<Todo> AddAsync(string task, CancellationToken ct) =>
        db.Todos.AddAsync(new Todo { Task = task }, ct);

    public Task<Todo?> GetAsync(string id, CancellationToken ct) =>
        db.Todos.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Todo> UpdateAsync(Todo t, CancellationToken ct) =>
        db.Todos.UpdateAsync(t, ct);

    public Task DeleteAsync(string id, CancellationToken ct) =>
        db.Todos.DeleteAsync(id, ct);
}
```

## Collection naming

By default, the collection name is the **pluralized snake-case** form of the
entity type name. `Todo` → `todos`. `TodoItem` → `todo_items`.

## Relationships — `Include` and `IncludeRef`

```csharp
// Book has a CategoryId pointing to a Category by ObjectId.
public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    [ReferenceTo(nameof(Category))] public string CategoryId { get; set; } = string.Empty;
    public Category? Category { get; set; }
}

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    [ForeignKeyTo(nameof(Book))] public List<Book> Books { get; set; } = [];
}

// Forward (parent → children):
var category = db.Categories.Include(c => c.Books).FirstOrDefault(c => c.Id == id);

// Backward (child → parent):
var book = db.Books.IncludeRef(b => b.Category).FirstOrDefault(b => b.Id == id);
```

## Auditing (opt-in)

```csharp
// 1) Enable
new MongoDbCoreOptions { EnableAuditing = true, ... };

// 2) Mark the entity
[Auditable]
public class Invoice : BaseEntity { /* ... */ }

// 3) Add/Update/Delete is automatically logged to the audits collection.

// 4) Resolve IAuditService to inspect:
public class AuditController(IAuditService audits) : ControllerBase
{
    public Task<(List<AuditEntity>, long)> GetAll(int page = 1)
        => audits.GetAllAsync(pageSize: 20, page: page);
}
```

## `SelfCachedCollection<T>` — read-heavy lookups

```csharp
public SelfCachedCollection<Country> Countries { get; set; } = null!;

// Reads come from an in-memory snapshot; writes go through MongoDB and
// refresh the cache under a lock. Eager loading (Include) isn't supported.
var country = db.Countries.FirstOrDefault(c => c.Code == "UZ");
```

## What's new in 2.0

See [CHANGELOG.md](CHANGELOG.md). Highlights: the package finally compiles
(the `Audit` / `AuditEntity` mismatch made 1.x unusable from a fresh restore),
auditing is opt-in, UTC timestamps, server-side `Select`, multi-target
`net8.0;net9.0`, dropped Blazor dependency.

## Building from source

```bash
dotnet restore MongoDbCore.sln
dotnet build MongoDbCore.sln --configuration Release
dotnet test tests/MongoDbCore.Tests/MongoDbCore.Tests.csproj --configuration Release
```

## License

MIT — see [LICENSE](LICENSE).

## Contributing

PRs welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

---
sidebar_position: 1
---

# Connecting to MongoDB

Define your entity model — inherit from `BaseEntity`:

```csharp
public class Todo : BaseEntity
{
    public string Task { get; set; } = string.Empty;
}
```

`BaseEntity` gives you a Mongo-friendly `Id` (24-char ObjectId hex) plus `CreatedAt` / `UpdatedAt` written as `DateTimeOffset.UtcNow`.

Create a class inheriting from `MongoDbContext` to define your database context:

```csharp
public class AppDbContext(MongoDbCoreOptions options) : MongoDbContext(options)
{
    public Collection<Todo> Todos { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        // Seed data / create indexes here.
        // This now runs to completion before the context is returned to DI.
        return Task.CompletedTask;
    }
}
```

Inject and use:

```csharp
public class TodoService(AppDbContext dbContext)
{
    // dbContext is ready to use
}
```

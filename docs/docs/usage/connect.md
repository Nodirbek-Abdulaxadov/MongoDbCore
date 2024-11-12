---
sidebar_position: 1
---

# Connecting to MongoDB

Define Entity Model: Create a class inheriting from BaseEntity for each model.

```csharp
public class Todo : BaseEntity 
{ 
    public string Task { get; set; } = string.Empty;
}
```

Setup Database Context: Create a class inheriting from MongoDbContext to define your database context.

```csharp
public class AppDbContext(MongoDbCoreOptions options) : MongoDbContext(options)
{ 
    public Collection<Todo> Todos { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        // Initialize data if necessary
    }
}
```

Using mongodb is very easy. Just inject your DbContext class and use it

```csharp

public class TodoService(AppDbContext dbContext)
{
    // now dbContext ready to use
}

```
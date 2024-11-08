# Basic Setup and Configuration
Define MongoDB Connection: Set up your MongoDB connection string in the application settings.

```json
"MongoDB": { 
    "Connection": "mongodb://localhost:27017", 
    "Database": "database_name", 
    "MaxConnectionPoolSize": 100 
}
```

Configure MongoDbCore Context: Initialize MongoDbCore in Program.cs.

```csharp
builder.Services.AddMongoDbContext<AppDbContext>
(builder.Configuration.GetSection("MongoDB").Get<MongoDbCoreOptions>()!);
```

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
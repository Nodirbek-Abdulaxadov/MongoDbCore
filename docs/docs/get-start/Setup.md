# Basic Setup and Configuration

Define your MongoDB connection in the application settings:

```json
"MongoDB": {
    // custom connection with username, password, host, port
    // "Connection": "mongodb://replace_username:replace_password@replace_hostname:replace_post"
    "Connection": "mongodb://localhost:27017",
    "Database": "database_name",
    // optional — defaults to 100
    "MaxConnectionPoolSize": 100,
    // optional — enables the [Auditable] feature (off by default)
    "EnableAuditing": false
}
```

> `MaxConnectionPoolSize` is a public `set` property in 2.0 — it now binds correctly from `IConfiguration.Get<MongoDbCoreOptions>()`.

Configure the MongoDbCore context in `Program.cs`:

```csharp
builder.Services.AddMongoDbContext<AppDbContext>(
    builder.Configuration
        .GetSection("MongoDB")
        .Get<MongoDbCoreOptions>()!);
```

The registration:
1. Creates a single `MongoClient` honouring `MaxConnectionPoolSize`.
2. Runs `HealthCheckDb()` so the app fails fast on bad connection strings.
3. Populates every `Collection<T>` / `SelfCachedCollection<T>` property on your `DbContext` via reflection.
4. Awaits your `OnInitializedAsync()` override before returning the context to DI.
5. If `EnableAuditing` is true, registers `IAuditService` and creates the `Audits` collection.

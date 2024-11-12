# Basic Setup and Configuration
Define MongoDB Connection: Set up your MongoDB connection string in the application settings.

```json
"MongoDB": {
    //custom connection with username, password, host, port
    //"Connection": "mongodb://replace_username:replace_password@replace_hostname:replace_post"
    "Connection": "mongodb://localhost:27072",
    "Database": "database_name", 
    //optional
    "MaxConnectionPoolSize": 100 
}
```

Configure MongoDbCore Context: Initialize MongoDbCore in Program.cs.

```csharp

builder.Services.AddMongoDbContext<AppDbContext>
(builder.Configuration.GetSection("MongoDB").Get<MongoDbCoreOptions>()!);

```
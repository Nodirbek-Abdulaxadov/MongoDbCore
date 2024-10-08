# MongoDbCore

## Latest stable version 1.3.0

[`https://www.nuget.org/packages/MongoDbCore/`](https://www.nuget.org/packages/MongoDbCore/)


MongoDbCore is a lightweight micro-ORM for .NET designed to simplify interactions with MongoDB. It provides a straightforward API for performing common database operations, enabling developers to build applications quickly and efficiently.

## Key Features

- **Easy Setup**: Simple configuration to get started with MongoDB.
- **CRUD Operations**: Supports Create, Read, Update, and Delete operations seamlessly.
- **Asynchronous Support**: Perform database operations asynchronously for better performance.
- **Performance Optimizations**: Designed for efficient use of memory and speed.

## Installation

To install MongoDbCore, add it to your project using NuGet:

```dotnet add package MongoDbCore```

## Getting Started

### Configuration

To configure MongoDbCore, follow these steps:

1. **Set Up Your MongoDB Connection**: Define your MongoDB connection string in your application settings.
```
"MongoDB": {
    "Connection": "mongodb://localhost:27017",
    "Database": "database_name",
    "MaxConnectionPoolSize": 100 //default
}
```

2. **Initialize MongoDbCore**: Configure the MongoDbCore context in your `Program.cs`:

```
// Setup the application and services
builder.Services.AddMongoDbContext<AppDbContext>(builder.Configuration
                                                        .GetSection("MongoDB")
                                                        .Get<MongoDbCoreOptions>()!);
```

### Create TodoEntity based on BaseEntity

Create a class that inherits from `MongoDbContext` to define your database context:

```
public class Todo : BaseEntity
{
    public string Task { get; set; } = string.Empty;
}
```

### Setting Up the Database Context

Create a class that inherits from `MongoDbContext` to define your database context:

```
public class AppDbContext(MongoDbCoreOptions options) : MongoDbContext(options)
{
    public Collection<Todo> Todos { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        // Initialize data if necessary
    }
}
```

### Basic CRUD Operations

MongoDbCore simplifies CRUD operations. Here's how you can use it:

#### Inject AppDbContext in your service class:

```
public class TodoService(AppDbContext dbContext)
{ }
```

#### Create
To create a new entity:

```
Todo newTodo = new();
dbContext.Todos.Add(newTodo);
```

#### Read
To retrieve an entity:

```
var todos = dbContext.Todos.ToList();
```

#### Update

To update an existing entity:

```
var updateTodo = dbContext.Todos.FirstOrDefault();
dbContext.Todos.Update(updateTodo);
```

#### Delete

To delete an entity:

```
var updateTodo = dbContext.Todos.FirstOrDefault();
dbContext.Todos.Delete(updateTodo);
```


## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please read the [CONTRIBUTING](CONTRIBUTING.md) guide for details on how to contribute to this project.

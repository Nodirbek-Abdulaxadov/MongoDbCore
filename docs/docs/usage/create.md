---
sidebar_position: 2
---

# Create

Create new Todo

```csharp

public class TodoService(AppDbContext dbContext)
{    
    public Todo Create(string task)
    {
        Todo todo = new() { Task = task };
        return dbContext.Todos.Add(todo);
    }

    public Task<Todo> CreateAsync(string task)
    {
        Todo todo = new() { Task = task };
        return dbContext.Todos.AddAsync(todo);
    }
}

```
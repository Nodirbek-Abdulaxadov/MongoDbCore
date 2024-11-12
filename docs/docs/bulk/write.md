---
sidebar_position: 1
---

# Bulk write

Create multiple todos

```csharp

    public IEnumerable<Todo> CreateRange(IEnumerable<string> tasks)
    {
        var todos = tasks.Select(x => new Todo { Task = x });
        return dbContext.Todos.AddRange(todos);
    }

    public Task<IEnumerable<Todo>> CreateRangeAsync(IEnumerable<string> tasks)
    {
        var todos = tasks.Select(x => new Todo { Task = x });
        return dbContext.Todos.AddRangeAsync(todos);
    }

```
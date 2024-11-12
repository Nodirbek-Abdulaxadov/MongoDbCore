---
sidebar_position: 2
---

# Where

Filter items

```csharp

    public IEnumerable<Todo> Filter(DateTime start, DateTime end)
    {
        var todos = dbContext.Todos.Where(x => x.CreatedAt >= start && x.CreatedAt <= end);
        return todos.ToList();
    }

```
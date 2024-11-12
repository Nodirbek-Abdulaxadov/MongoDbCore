---
sidebar_position: 3
---

# Bulk delete

Delete multiple todos

```csharp

    public void DeleteRange(IEnumerable<Todo> todos)
        => dbContext.Todos.DeleteRange(todos);

    public Task DeleteRangeAsync(IEnumerable<Todo> todos)
        => dbContext.Todos.DeleteRangeAsync(todos);

```
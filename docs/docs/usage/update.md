---
sidebar_position: 4
---

# Update

Update todos

```csharp

    public Todo Update(Todo todo)
        => dbContext.Todos.Update(todo);

    public Task<Todo> UpdateAsync(Todo todo)
        => dbContext.Todos.UpdateAsync(todo);

```
---
sidebar_position: 5
---

# Delete

Delete todo

```csharp

    public void Delete(Todo todo)
        => dbContext.Todos.Delete(todo);

    public Task DeleteAsync(Todo todo)
        => dbContext.Todos.DeleteAsync(todo);

    public void DeleteById(string id)
        => dbContext.Todos.Delete(id);
        
    public Task DeleteByIdAsync(string id)
        => dbContext.Todos.DeleteAsync(id);

```
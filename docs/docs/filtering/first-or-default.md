---
sidebar_position: 1
---

# FirstOrDefault

Get first or default item

```csharp

    public Todo? GetById(string id)
        => dbContext.Todos.FirstOrDefault(x => x.Id == id);

    public Task<Todo>? GetByIdAsync(string id)
        => dbContext.Todos.FirstOrDefaultAsync(x => x.Id == id);

```
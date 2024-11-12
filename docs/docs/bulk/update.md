---
sidebar_position: 2
---

# Bulk update

Update multiple todos

```csharp

    public long UpdateMany()
        => dbContext.Todos.UpdateMany(x => x.CreatedAt > DateTime.Now,
                                  x => x.Task, "Task updated!");
    public Task<long> UpdateManyAsync()
        => dbContext.Todos.UpdateManyAsync(x => x.CreatedAt > DateTime.Now,
                                        x => x.Task, "Task updated!");

```
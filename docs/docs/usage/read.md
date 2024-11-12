---
sidebar_position: 3
---

# Read

Read todos all

```csharp

    public IEnumerable<Todo> GetAll()
        => dbContext.Todos.ToList();

    public async Task<IEnumerable<Todo>> GetAllAsync()
        => await dbContext.Todos.ToListAsync();
    
```
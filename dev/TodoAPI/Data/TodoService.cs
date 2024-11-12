namespace TodoAPI.Data;

public class TodoService(AppDbContext dbContext)
{
    #region Create

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

    #endregion

    #region Read

    public IEnumerable<Todo> GetAll()
    => dbContext.Todos.ToList();

    public async Task<IEnumerable<Todo>> GetAllAsync()
        => await dbContext.Todos.ToListAsync();

    #endregion

    #region Update

    public Todo Update(Todo todo)
        => dbContext.Todos.Update(todo);

    public Task<Todo> UpdateAsync(Todo todo)
        => dbContext.Todos.UpdateAsync(todo);

    #endregion

    #region Delete

    public void Delete(Todo todo)
        => dbContext.Todos.Delete(todo);
    public Task DeleteAsync(Todo todo)
        => dbContext.Todos.DeleteAsync(todo);

    public void DeleteById(string id)
        => dbContext.Todos.Delete(id);
    public Task DeleteByIdAsync(string id)
        => dbContext.Todos.DeleteAsync(id);

    #endregion

    #region FirstOrDefault
    public Todo? GetById(string id)
    => dbContext.Todos.FirstOrDefault(x => x.Id == id);

    public Task<Todo>? GetByIdAsync(string id)
        => dbContext.Todos.FirstOrDefaultAsync(x => x.Id == id);

    #endregion

    #region Where
    public IEnumerable<Todo> Filter(DateTime start, DateTime end)
    {
        var todos = dbContext.Todos.Where(x => x.CreatedAt >= start && x.CreatedAt <= end);
        return todos.ToList();
    }
    #endregion

    #region BulkWrite

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

    #endregion

    #region BulkUpdate

    public long UpdateMany()
        => dbContext.Todos.UpdateMany(x => x.CreatedAt > DateTime.Now,
                                      x => x.Task, "Task updated!");
    public Task<long> UpdateManyAsync()
        => dbContext.Todos.UpdateManyAsync(x => x.CreatedAt > DateTime.Now,
                                           x => x.Task, "Task updated!");

    #endregion

    #region BulkDelete

    public void DeleteRange(IEnumerable<Todo> todos)
    => dbContext.Todos.DeleteRange(todos);

    public Task DeleteRangeAsync(IEnumerable<Todo> todos)
        => dbContext.Todos.DeleteRangeAsync(todos); 

    #endregion
}
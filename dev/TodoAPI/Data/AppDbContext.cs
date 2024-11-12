namespace TodoAPI.Data;

public class AppDbContext(MongoDbCoreOptions options) 
    : MongoDbContext(options)
{
    public Collection<Todo> Todos { get; set; } = null!;
}
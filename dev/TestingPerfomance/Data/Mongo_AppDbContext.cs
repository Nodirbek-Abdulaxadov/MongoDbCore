namespace TestingPerfomance;

public class Mongo_AppDbContext : MongoDbContext
{
    public Collection<Book> Books { get; set; } = null!;
}
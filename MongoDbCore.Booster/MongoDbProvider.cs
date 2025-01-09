namespace MongoDbCore.Booster;

public class MongoDbProvider<TDbContext> where TDbContext : MongoDbContext
{
    public MongoDbProvider(TDbContext dbContext)
    {
        
    }
}
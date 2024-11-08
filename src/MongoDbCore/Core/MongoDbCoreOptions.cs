namespace MongoDbCore;

public class MongoDbCoreOptions
{
    public string Connection { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "test";
    internal int MaxConnectionPoolSize { get; init; } = 100;

    public MongoDbCoreOptions() { }

    public MongoDbCoreOptions(string connection, string database, int maxConnectionPoolSize = 100)
    {
        Connection = connection;
        Database = database;
        MaxConnectionPoolSize = maxConnectionPoolSize;
    }
}
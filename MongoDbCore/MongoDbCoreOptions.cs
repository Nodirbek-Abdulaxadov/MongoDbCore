namespace MongoDbCore;

public class MongoDbCoreOptions
{
    public string Connection { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "test";

    public MongoDbCoreOptions() { }

    public MongoDbCoreOptions(string connection, string database)
    {
        Connection = connection;
        Database = database;
    }
}
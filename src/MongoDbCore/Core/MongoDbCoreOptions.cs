namespace MongoDbCore;

/// <summary>
/// Configuration values used to create a <see cref="MongoDbContext"/>.
/// Bindable from <c>IConfiguration</c> via <c>.Get&lt;MongoDbCoreOptions&gt;()</c>.
/// </summary>
public class MongoDbCoreOptions
{
    /// <summary>MongoDB connection string. Defaults to <c>mongodb://localhost:27017</c>.</summary>
    public string Connection { get; set; } = "mongodb://localhost:27017";

    /// <summary>Database name.</summary>
    public string Database { get; set; } = "test";

    /// <summary>Maximum connection pool size. Defaults to 100.</summary>
    public int MaxConnectionPoolSize { get; set; } = 100;

    /// <summary>When true, an <c>audits</c> collection is created and used for entities marked with <see cref="Auditable"/>.</summary>
    public bool EnableAuditing { get; set; }

    public MongoDbCoreOptions() { }

    public MongoDbCoreOptions(string connection, string database, int maxConnectionPoolSize = 100, bool enableAuditing = false)
    {
        Connection = connection;
        Database = database;
        MaxConnectionPoolSize = maxConnectionPoolSize;
        EnableAuditing = enableAuditing;
    }
}

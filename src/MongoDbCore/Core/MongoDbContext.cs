namespace MongoDbCore;

public abstract class MongoDbContext
{
    private readonly MongoDbCoreOptions _options;
    private IMongoClient _client;
    private static IMongoDatabase? _staticDatabase;
    public readonly IMongoDatabase Database;

    public MongoDbContext() : this(new MongoDbCoreOptions()) { }

    public MongoDbContext(MongoDbCoreOptions options)
    {
        _options = options;

        var mongoClientSettings = MongoClientSettings.FromConnectionString(_options.Connection);
        mongoClientSettings.MaxConnectionPoolSize = options.MaxConnectionPoolSize;

        _client = new MongoClient(mongoClientSettings);

        Database = _client.GetDatabase(_options.Database);
        _staticDatabase = Database;
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return Database.GetCollection<T>(name);
    }

    public IMongoCollection<T> GetCollection<T>()
        where T : BaseEntity
    {
        var collectionName = typeof(T).GetCollectionName<T>();
        return Database.GetCollection<T>(collectionName);
    }

    public static IMongoCollection<T> GetStaticCollection<T>(string name)
    {
        return _staticDatabase!.GetCollection<T>(name);
    }

    public void DropCollection(string name)
    {
        Database.DropCollection(name);
    }

    public async Task DropCollectionAsync(string name, CancellationToken cancellationToken = default)
    {
        await Database.DropCollectionAsync(name, cancellationToken);
    }

    public void HealthCheckDB()
    {
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var databases = _client.ListDatabaseNames(cts.Token);
        }
        catch (Exception ex)
        {
            throw new Exception($"Couldn't connect to MongoDB server! {ex.Message}");
        }
    }

    internal void Initialize() => OnInitialized();

    protected virtual void OnInitialized() => OnInitializedAsync();

    protected virtual Task OnInitializedAsync() => Task.CompletedTask;
}
namespace MongoDbCore;

public abstract class MongoDbContext
{
    private readonly MongoDbCoreOptions _options;
    private IMongoClient _client;
    private IMongoDatabase _database;

    public MongoDbContext() : this(new MongoDbCoreOptions()) { }

    public MongoDbContext(MongoDbCoreOptions options)
    {
        _options = options;
        _client = new MongoClient(_options.Connection);
        _database = _client.GetDatabase(_options.Database);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
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
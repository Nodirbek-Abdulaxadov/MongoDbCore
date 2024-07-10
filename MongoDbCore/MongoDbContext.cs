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

    internal void Initialize() => OnInitialized();

    protected virtual void OnInitialized() => OnInitializedAsync();

    protected virtual Task OnInitializedAsync() => Task.CompletedTask;
}
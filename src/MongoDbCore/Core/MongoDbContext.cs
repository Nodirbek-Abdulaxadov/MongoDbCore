namespace MongoDbCore;

/// <summary>
/// Base class for a MongoDB context. Inherit from this type and expose
/// <see cref="Collection{T}"/> or <see cref="SelfCachedCollection{T}"/>
/// properties for each persisted entity.
/// </summary>
public abstract class MongoDbContext
{
    private readonly MongoDbCoreOptions _options;
    private readonly IMongoClient _client;

    /// <summary>Underlying MongoDB database handle.</summary>
    public readonly IMongoDatabase Database;

    /// <summary>True when auditing is enabled on this context.</summary>
    public bool AuditingEnabled => _options.EnableAuditing;

    /// <summary>
    /// Audit log collection. Only populated when <see cref="MongoDbCoreOptions.EnableAuditing"/>
    /// is <c>true</c> at registration time.
    /// </summary>
    public Collection<AuditEntity>? Audits { get; internal set; }

    protected MongoDbContext() : this(new MongoDbCoreOptions()) { }

    protected MongoDbContext(MongoDbCoreOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        var settings = MongoClientSettings.FromConnectionString(_options.Connection);
        settings.MaxConnectionPoolSize = _options.MaxConnectionPoolSize;

        _client = new MongoClient(settings);
        Database = _client.GetDatabase(_options.Database);
    }

    /// <summary>Returns a raw MongoDB collection handle by name.</summary>
    public IMongoCollection<T> GetCollection<T>(string name)
        => Database.GetCollection<T>(name);

    /// <summary>Returns a raw MongoDB collection handle using the conventional pluralized snake-case name.</summary>
    public IMongoCollection<T> GetCollection<T>() where T : BaseEntity
        => Database.GetCollection<T>(typeof(T).GetCollectionName<T>());

    /// <summary>Drops the specified collection.</summary>
    public void DropCollection(string name) => Database.DropCollection(name);

    /// <summary>Drops the specified collection asynchronously.</summary>
    public Task DropCollectionAsync(string name, CancellationToken cancellationToken = default)
        => Database.DropCollectionAsync(name, cancellationToken);

    /// <summary>
    /// Verifies that the MongoDB server is reachable. Throws
    /// <see cref="InvalidOperationException"/> on failure.
    /// </summary>
    /// <param name="timeout">Optional timeout (default 5s).</param>
    public void HealthCheckDb(TimeSpan? timeout = null)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(5));
            _ = _client.ListDatabaseNames(cts.Token);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not connect to MongoDB server: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Called once after the context is fully wired up. Override to seed data,
    /// configure indexes, etc.
    /// </summary>
    protected virtual Task OnInitializedAsync() => Task.CompletedTask;

    internal void Initialize() => OnInitializedAsync().GetAwaiter().GetResult();
}

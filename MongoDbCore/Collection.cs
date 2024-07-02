namespace MongoDbCore;

public class Collection<T> where T : BaseEntity
{
    #region Initialize

    private readonly IMongoCollection<T>? _collection;
    private readonly bool _isCacheable;
    private IFindFluent<T, T>? _cache;

    public Collection(MongoDbContext dbContext)
    {
        _collection = dbContext.GetCollection<T>(typeof(T).Name.Pluralize().Underscore());
        _isCacheable = CheckCacheablityOfTEntity();
    }

    #endregion

    #region Queries

    public List<T> ToList()
        => Get().ToList();

    public Task<List<T>> ToListAsync()
        => Get().ToListAsync();

    public T FirstOrDefault()
        => Get().FirstOrDefault();

    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        => Get().FirstOrDefault();

    public Task<T> FirstOrDefaultAsync()
        => Get().FirstOrDefaultAsync();

    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => Get().FirstOrDefaultAsync();

    public bool Any()
        => Get().Any();

    public bool Any(Expression<Func<T, bool>> predicate)
        => Get().Any();

    public long Count()
        => _collection!.CountDocuments(FilterDefinition<T>.Empty);

    #endregion

    #region Mutation

    public void Add(T entity)
    {
        if (_isCacheable) UpdateCache();
        _collection!.InsertOne(entity);
    }

    public Task AddAsync(T entity)
    {
        if (_isCacheable) UpdateCache();
        return _collection!.InsertOneAsync(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        if (_isCacheable) UpdateCache();
        _collection!.InsertMany(entities);
    }

    public Task AddRangeAsync(IEnumerable<T> entities)
    {
        if (_isCacheable) UpdateCache();
        return _collection!.InsertManyAsync(entities);
    }

    public void Update(T entity)
    {
        if (_isCacheable) UpdateCache();
        _collection!.ReplaceOne(x => x.Id == entity.Id, entity);
    }

    public Task UpdateAsync(T entity)
    {
        if (_isCacheable) UpdateCache();
        return _collection!.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public void Delete(string id)
    {
        if (_isCacheable) UpdateCache();
        _collection!.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, id));
    }

    public Task DeleteAsync(string id)
    {
        if (_isCacheable) UpdateCache();
        return _collection!.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id));
    }

    #endregion

    #region Extensions

    public IFindFluent<T, T> AsFindFluent()
        => _collection.Find(_ => true);

    public IFindFluent<T, T> Where(Expression<Func<T, bool>> predicate)
        => _collection.Find(predicate);

    #endregion

    #region Helpers

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string? ToString()
    {
        return base.ToString();
    }

    private bool CheckCacheablityOfTEntity()
        => typeof(T).GetCustomAttributes(typeof(Cacheable), true).Any();

    private void UpdateCache()
    {
        _cache = _collection.Find(FilterDefinition<T>.Empty);
    }

    private IFindFluent<T, T> Get()
    {
        if (!_isCacheable)
        {
            return _collection.Find(FilterDefinition<T>.Empty);
        }

        if (_cache is null)
        {
            return _collection.Find(FilterDefinition<T>.Empty);
        }

        return _cache;
    }

    #endregion
}
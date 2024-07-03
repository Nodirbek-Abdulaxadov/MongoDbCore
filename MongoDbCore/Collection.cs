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
        => Get(predicate).FirstOrDefault();

    public Task<T> FirstOrDefaultAsync()
        => Get().FirstOrDefaultAsync();

    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => Get(predicate).FirstOrDefaultAsync();

    public bool Any()
        => Get().Any();

    public bool Any(Expression<Func<T, bool>> predicate)
        => Get(predicate).Any();

    public long Count()
        => _collection!.CountDocuments(FilterDefinition<T>.Empty);

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        _collection!.InsertOne(entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public async Task<T> AddAsync(T entity)
    {
        await _collection!.InsertOneAsync(entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        _collection!.InsertMany(entities);
        if (_isCacheable) UpdateCache();
        return entities;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await _collection!.InsertManyAsync(entities);
        if (_isCacheable) UpdateCache();
        return entities;
    }

    public T Update(T entity)
    {
        _collection!.ReplaceOne(x => x.Id == entity.Id, entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await _collection!.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public void Delete(string id)
    {
        _collection!.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, id));
        if (_isCacheable) UpdateCache();
    }

    public async Task DeleteAsync(string id)
    {
        await _collection!.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id));
        if (_isCacheable) UpdateCache();
    }

    public void Delete(T entity)
        => Delete(entity.Id);

    public Task DeleteAsync(T entity)
        => DeleteAsync(entity.Id);

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

    private IFindFluent<T, T> Get(FilterDefinition<T>? filter = null)
    {
        if (!_isCacheable)
        {
            return _collection.Find(filter ?? FilterDefinition<T>.Empty);
        }

        if (_cache is null)
        {
            _cache = _collection.Find(filter ?? FilterDefinition<T>.Empty);
        }
        var list = _cache.ToList();
        return _cache;
    }

    #endregion
}
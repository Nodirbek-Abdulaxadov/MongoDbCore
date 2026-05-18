namespace MongoDbCore;

/// <summary>
/// Collection variant that keeps an in-memory snapshot of every document for
/// read-optimized scenarios. Mutations update the underlying MongoDB
/// collection and refresh the cache under a single lock.
/// </summary>
/// <remarks>
/// Eager loading (<c>Include</c>) is not supported by this collection type.
/// </remarks>
public class SelfCachedCollection<T> where T : BaseEntity
{
    #region Initialize

    internal readonly IMongoCollection<T> Source;
    private List<T> _cache;
    private readonly object _lock = new();
    public readonly string CollectionName;

    public SelfCachedCollection(MongoDbContext dbContext)
    {
        if (dbContext is null) throw new ArgumentNullException(nameof(dbContext));
        CollectionName = typeof(T).Name.Pluralize().Underscore();
        Source = dbContext.GetCollection<T>(CollectionName);
        _cache = Source.Find(FilterDefinition<T>.Empty).ToList();
    }

    #endregion

    #region Queries

    public List<T> ToList()
    {
        lock (_lock) return [.._cache];
    }

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<List<T>>(cancellationToken);
        lock (_lock) return Task.FromResult<List<T>>([.._cache]);
    }

    public T? FirstOrDefault()
    {
        lock (_lock) return _cache.FirstOrDefault();
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var compiled = predicate.Compile();
        lock (_lock) return _cache.FirstOrDefault(compiled);
    }

    public Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<T?>(cancellationToken);
        lock (_lock) return Task.FromResult<T?>(_cache.FirstOrDefault());
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<T?>(cancellationToken);
        var compiled = predicate.Compile();
        lock (_lock) return Task.FromResult<T?>(_cache.FirstOrDefault(compiled));
    }

    public List<T> Where(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var compiled = predicate.Compile();
        lock (_lock) return _cache.Where(compiled).ToList();
    }

    public bool Any()
    {
        lock (_lock) return _cache.Count > 0;
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var compiled = predicate.Compile();
        lock (_lock) return _cache.Any(compiled);
    }

    public List<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var compiled = selector.Compile();
        lock (_lock) return _cache.Select(compiled).ToList();
    }

    public List<TResult> SelectMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var compiled = selector.Compile();
        lock (_lock) return _cache.SelectMany(compiled).ToList();
    }

    public long Count()
    {
        lock (_lock) return _cache.Count;
    }

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        EnsureValidId(entity);
        TouchTimestamps(entity, isUpdate: false);
        lock (_lock)
        {
            Source.InsertOne(entity);
            _cache.Add(entity);
        }
        return entity;
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<T>(cancellationToken);
        return Task.FromResult(Add(entity));
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        var list = entities as IList<T> ?? entities.ToList();
        foreach (var e in list)
        {
            EnsureValidId(e);
            TouchTimestamps(e, isUpdate: false);
        }
        lock (_lock)
        {
            Source.InsertMany(list);
            _cache.AddRange(list);
        }
        return list;
    }

    public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<IEnumerable<T>>(cancellationToken);
        return Task.FromResult(AddRange(entities));
    }

    public T Update(T entity)
    {
        TouchTimestamps(entity, isUpdate: true);
        lock (_lock)
        {
            Source.ReplaceOne(x => x.Id == entity.Id, entity);
            var idx = _cache.FindIndex(x => x.Id == entity.Id);
            if (idx >= 0) _cache[idx] = entity; else _cache.Add(entity);
        }
        return entity;
    }

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<T>(cancellationToken);
        return Task.FromResult(Update(entity));
    }

    public long UpdateMany<TProperty>(Expression<Func<T, bool>> filter,
                                      Expression<Func<T, TProperty>> propertySelection,
                                      TProperty newValue)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(propertySelection);

        var now = DateTimeOffset.UtcNow;
        var update = Builders<T>.Update
            .Set(propertySelection, newValue)
            .Set(t => t.UpdatedAt, now);

        lock (_lock)
        {
            var result = Source.UpdateMany(filter, update);
            if (result.ModifiedCount > 0 &&
                propertySelection.Body is MemberExpression mem &&
                mem.Member is PropertyInfo propInfo)
            {
                var compiled = filter.Compile();
                foreach (var item in _cache.Where(compiled))
                {
                    propInfo.SetValue(item, newValue);
                    item.UpdatedAt = now;
                }
            }
            return result.ModifiedCount;
        }
    }

    public Task<long> UpdateManyAsync<TProperty>(Expression<Func<T, bool>> filter,
                                                 Expression<Func<T, TProperty>> propertySelection,
                                                 TProperty newValue,
                                                 CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<long>(cancellationToken);
        return Task.FromResult(UpdateMany(filter, propertySelection, newValue));
    }

    public void Delete(string id)
    {
        lock (_lock)
        {
            Source.DeleteOne(x => x.Id == id);
            _cache.RemoveAll(x => x.Id == id);
        }
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
        Delete(id);
        return Task.CompletedTask;
    }

    public void Delete(T entity) => Delete(entity.Id);

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        => DeleteAsync(entity.Id, cancellationToken);

    public void DeleteAll()
    {
        lock (_lock)
        {
            Source.DeleteMany(FilterDefinition<T>.Empty);
            _cache.Clear();
        }
    }

    public void ReloadCache()
    {
        lock (_lock)
        {
            _cache = Source.Find(FilterDefinition<T>.Empty).ToList();
        }
    }

    #endregion

    #region Helpers

    private static void EnsureValidId(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id) || !ObjectId.TryParse(entity.Id, out _))
            entity.Id = BaseEntity.NewId;
    }

    private static void TouchTimestamps(T entity, bool isUpdate)
    {
        var now = DateTimeOffset.UtcNow;
        if (!isUpdate) entity.CreatedAt = now;
        entity.UpdatedAt = now;
    }

    #endregion
}

namespace MongoDbCore;

/// <summary>
/// Cached Collection for best read time
/// References are not available now
/// </summary>
/// <typeparam name="T"></typeparam>
public class SelfCachedCollection<T> where T : BaseEntity
{
    #region Initialize

    internal readonly IMongoCollection<T>? Source;
    private List<T> _cache = [];
    private readonly object _lock = new();
    public readonly string CollectionName = string.Empty;

    public SelfCachedCollection(MongoDbContext dbContext)
    {
        CollectionName = typeof(T).Name.Pluralize().Underscore();
        Source = dbContext.GetCollection<T>(CollectionName);
        lock (_lock)
        {
            _cache = ToList();
        }
    }

    #endregion

    #region Queries

    public List<T> ToList()
    {
        lock (_lock)
        {
            return new(_cache);
        }
    }

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // Check if the cancellation has already been requested
            if (cancellationToken.IsCancellationRequested)
            {
                // Throw a TaskCanceledException to indicate the operation was canceled
                return Task.FromCanceled<List<T>>(cancellationToken);
            }

            // Simulate async work and return the list
            return Task.FromResult(_cache);
        }
    }

    public T? FirstOrDefault()
    {
        lock (_lock)
        {
            return _cache.FirstOrDefault();
        }
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        lock (_lock)
        {
            if (_cache.Count == 0 || predicate is null)
            {
                return null;
            }
            var compiledPredicate = predicate.Compile();

            return _cache.Where(item => item != null).FirstOrDefault(compiledPredicate);
        }
    }

    public List<T> Where(Expression<Func<T, bool>> predicate)
    {
        lock (_lock)
        {
            var compiledPredicate = predicate.Compile();
            var filtered = _cache.Where(compiledPredicate).ToList();
            return filtered;
        }
    }

    public Task<T>? FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // Check if the cancellation has already been requested
            if (cancellationToken.IsCancellationRequested)
            {
                // Throw a TaskCanceledException to indicate the operation was canceled
                return Task.FromCanceled<T>(cancellationToken);
            }

            // Simulate async work and return the list
            if (_cache.Count == 0)
            {
                return null;
            }

            return Task.FromResult(_cache.First());
        }
    }

    public Task<T?>? FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // Check if the cancellation has already been requested
            if (cancellationToken.IsCancellationRequested)
            {
                // Throw a TaskCanceledException to indicate the operation was canceled
                return Task.FromCanceled<T?>(cancellationToken);
            }

            // Simulate async work and return the list
            if (_cache.Count == 0)
            {
                return null;
            }

            var compiledPredicate = predicate.Compile();
            return Task.FromResult(_cache.Where(compiledPredicate).FirstOrDefault());
        }
    }

    public bool Any()
    {
        lock (_lock)
        {
            return _cache.Count > 0;
        }
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        lock (_lock)
        {
            var compiledPredicate = predicate.Compile();
            return _cache.Where(compiledPredicate).Any();
        }
    }

    public List<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        lock (_lock)
        {
            var compiledSelector = selector.Compile();
            return _cache.Select(compiledSelector).ToList();
        }
    }

    public List<TResult> SelectMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector)
    {
        lock (_lock)
        {
            var compiledSelector = selector.Compile();
            return _cache.SelectMany(compiledSelector).ToList();
        }
    }

    public long Count()
    {
        lock (_lock)
        {
            return _cache.Count;
        }
    }

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = BaseEntity.NewId;
            }
            Source!.InsertOne(entity);
            ReloadCache();
            return entity;
        }
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Throw a TaskCanceledException to indicate the operation was canceled
                return Task.FromCanceled<T>(cancellationToken);
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = BaseEntity.NewId;
            }

            Source!.InsertOne(entity);
            ReloadCache();
            return Task.FromResult(entity);
        }
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        lock (_lock)
        {
            Source!.InsertMany(entities);
            ReloadCache();
            return entities;
        }
    }

    public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Throw a TaskCanceledException to indicate the operation was canceled
                return Task.FromCanceled<IEnumerable<T>>(cancellationToken);
            }

            Source!.InsertMany(entities);
            ReloadCache();
            return Task.FromResult(entities);
        }
    }

    public T Update(T entity)
    {
        lock (_lock)
        {
            Source!.ReplaceOne(x => x.Id == entity.Id, entity);
            ReloadCache();
            return entity;
        }
    }

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Throw a TaskCanceledException to indicate the operation was canceled
                return Task.FromCanceled<T>(cancellationToken);
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = BaseEntity.NewId;
            }

            Source!.ReplaceOne(x => x.Id == entity.Id, entity);
            ReloadCache();
            return Task.FromResult(entity);
        }
    }

    public long UpdateMany<TProperty>(Expression<Func<T, bool>> filter,
                                  Expression<Func<T, TProperty>> propertySelection,
                                  TProperty newValue)
    {
        if (Source is null)
            throw new InvalidOperationException("The data source is not initialized.");

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (propertySelection is null)
            throw new ArgumentNullException(nameof(propertySelection));

        lock (_lock)
        {
            try
            {
                // Update in the source collection
                var update = Builders<T>.Update
                    .Set(propertySelection, newValue)
                    .Set(t => t.UpdatedAt, DateTime.Now);
                var result = Source.UpdateMany(filter, update);

                if (result.ModifiedCount > 0)
                {
                    // Compile the filter into a predicate to update the in-memory list
                    var compiledFilter = filter.Compile();
                    foreach (var item in _cache.Where(compiledFilter))
                    {
                        var propertyInfo = ((MemberExpression)propertySelection.Body).Member as PropertyInfo;
                        if (propertyInfo != null)
                        {
                            propertyInfo.SetValue(item, newValue);
                            item.UpdatedAt = DateTime.Now;
                        }
                    }
                }
                ReloadCache();

                return result.ModifiedCount;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating documents.", ex);
            }
        }
    }

    public Task<long> UpdateManyAsync<TProperty>(Expression<Func<T, bool>> filter,
                                                       Expression<Func<T, TProperty>> propertySelection,
                                                       TProperty newValue,
                                                       CancellationToken cancellationToken = default)
    {
        if (Source is null)
            throw new InvalidOperationException("The data source is not initialized.");

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (propertySelection is null)
            throw new ArgumentNullException(nameof(propertySelection));

        // Lock for thread-safe access to the in-memory list
        lock (_lock)
        {
            // Compile the filter into a predicate to update the in-memory list later
            var compiledFilter = filter.Compile();

            try
            {
                // Create the update definition
                var update = Builders<T>.Update
                    .Set(propertySelection, newValue)
                    .Set(t => t.UpdatedAt, DateTime.Now);

                // Update the source collection asynchronously
                var result = Source.UpdateMany(filter, update, null, cancellationToken);

                if (result.ModifiedCount > 0)
                {
                    // Update matching entities in the in-memory list
                    foreach (var item in _cache.Where(compiledFilter))
                    {
                        var propertyInfo = ((MemberExpression)propertySelection.Body).Member as PropertyInfo;
                        if (propertyInfo != null)
                        {
                            propertyInfo.SetValue(item, newValue);
                            item.UpdatedAt = DateTime.Now;
                        }
                    }
                }
                ReloadCache();

                return Task.FromResult(result.ModifiedCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating documents asynchronously.", ex);
            }
        }
    }

    public void Delete(string id)
    {
        lock (_lock)
        {
            var item = _cache.Find(x => x.Id == id);
            if (item != null)
            {
                Source!.DeleteOne(x => x.Id == id);
                ReloadCache();
            }
        }
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var item = _cache.Find(x => x.Id == id);
                if (item != null)
                {
                    Source!.DeleteOne(x => x.Id == id);
                    ReloadCache();
                }
            }

            return Task.CompletedTask;
        }
    }

    public void Delete(T entity)
    {
        lock (_lock)
        {
            var item = FirstOrDefault(x => x.Id == entity.Id);
            if (item is not null)
            {
                Source!.DeleteOne(x => x.Id == entity.Id);
                ReloadCache();
            }
        }
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var item = _cache.FirstOrDefault(x => x.Id == entity.Id);
                if (item != null)
                {
                    Source!.DeleteOne(x => x.Id == item.Id);
                    ReloadCache();
                }
            }

            return Task.CompletedTask;
        }
    }

    public void DeleteAll()
    {
        Source!.DeleteMany(FilterDefinition<T>.Empty);
        _cache.Clear();
    }

    public void ReloadCache()
    {
        lock (_lock)
        {
            _cache.Clear();
            _cache = Source!.Find(FilterDefinition<T>.Empty).ToList();
        }
    }

    #endregion
}
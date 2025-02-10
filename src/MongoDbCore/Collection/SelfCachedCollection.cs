namespace MongoDbCore;

/// <summary>
/// Cached Collection for best read time
/// References are not available now
/// </summary>
/// <typeparam name="T"></typeparam>
public class SelfCachedCollection<T> where T : BaseEntity
{
    #region Initialize

    public readonly IMongoCollection<T>? Source;
    private List<T> _cache = [];
    private List<T> _temp = [];
    private bool _writeInProccess = false;

    public string CollectionName => typeof(T).Name.Pluralize().Underscore();

    public SelfCachedCollection(MongoDbContext dbContext)
    {
        Source = dbContext.GetCollection<T>(CollectionName);
        _cache = ToList();
    }

    #endregion

    #region Queries

    public List<T> ToList()
    {
        if (_writeInProccess)
        {
            return _temp;
        }

        return _cache;
    }

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        // Check if the cancellation has already been requested
        if (cancellationToken.IsCancellationRequested)
        {
            // Throw a TaskCanceledException to indicate the operation was canceled
            return Task.FromCanceled<List<T>>(cancellationToken);
        }

        if (_writeInProccess)
        {
            return Task.FromResult(_temp);
        }

        return Task.FromResult(_cache);
    }

    public T? FirstOrDefault()
    {
        if (_writeInProccess)
        {
            return _temp.FirstOrDefault();
        }

        return _cache.FirstOrDefault();
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        if (_temp.Count == 0 || predicate is null)
        {
            return null;
        }
        var compiledPredicate = predicate.Compile();

        if (_writeInProccess)
        {
            return _temp.FirstOrDefault(compiledPredicate);
        }

        return _cache.Where(item => item != null).FirstOrDefault(compiledPredicate);
    }

    public List<T> Where(Expression<Func<T, bool>> predicate)
    {
        var compiledPredicate = predicate.Compile();
        if (_writeInProccess)
        {
            return _temp.Where(compiledPredicate).ToList();
        }
        
        return _cache.Where(compiledPredicate).ToList();
    }

    public Task<T>? FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        // Check if the cancellation has already been requested
        if (cancellationToken.IsCancellationRequested)
        {
            // Throw a TaskCanceledException to indicate the operation was canceled
            return Task.FromCanceled<T>(cancellationToken);
        }

        // Simulate async work and return the list
        if (_temp.Count == 0)
        {
            return null;
        }

        if (_writeInProccess)
        {
            return Task.FromResult(_temp.First());
        }

        return Task.FromResult(_cache.First());
    }

    public Task<T>? FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Check if the cancellation has already been requested
        if (cancellationToken.IsCancellationRequested)
        {
            // Throw a TaskCanceledException to indicate the operation was canceled
            return Task.FromCanceled<T>(cancellationToken);
        }

        // Simulate async work and return the list
        if (_temp.Count == 0)
        {
            return null;
        }

        var compiledPredicate = predicate.Compile();

        if (_writeInProccess)
        {
            return Task.FromResult(_temp.Where(compiledPredicate).First());
        }

        return Task.FromResult(_cache.Where(compiledPredicate).First());
    }

    public bool Any()
    {
        if (_writeInProccess)
        {
            return _temp.Count > 0;
        }

        return _temp.Count > 0;
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        var compiledPredicate = predicate.Compile();

        if (_writeInProccess)
        {
            return _temp.Any(compiledPredicate);
        }

        return _cache.Where(compiledPredicate).Any();
    }

    public List<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        var compiledSelector = selector.Compile();

        if (_writeInProccess)
        {
            return _temp.Select(compiledSelector).ToList();
        }

        return _cache.Select(compiledSelector).ToList();
    }

    public List<TResult> SelectMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector)
    {
        var compiledSelector = selector.Compile();

        if (_writeInProccess)
        {
            return _temp.SelectMany(compiledSelector).ToList();
        }

        return _cache.SelectMany(compiledSelector).ToList();
    }

    public long Count()
    {
        if (_writeInProccess)
        {
            return _temp.Count;
        }
        return _temp.Count;
    }

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = BaseEntity.NewId;
        }
        Source!.InsertOne(entity);
        ReloadCache();
        return entity;
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
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

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        Source!.InsertMany(entities);
        ReloadCache();
        return entities;
    }

    public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
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

    public T Update(T entity)
    {
        Source!.ReplaceOne(x => x.Id == entity.Id, entity);
        ReloadCache();
        return entity;
    }

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
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
                        item.UpdatedAt = DateTime.Now; // Assuming UpdatedAt exists in the type
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
                        item.UpdatedAt = DateTime.Now; // Assuming UpdatedAt exists in the type
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


    public void Delete(string id)
    {
        var item = _cache.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            Source!.DeleteOne(x => x.Id == id);
            ReloadCache();
        }
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            var item = _cache.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                Source!.DeleteOne(x => x.Id == id);
                ReloadCache();
            }
        }

        return Task.CompletedTask;
    }

    public void Delete(T entity)
    {
        var item = FirstOrDefault(x => x.Id == entity.Id);
        if (item is not null)
        {
            Source!.DeleteOne(x => x.Id == entity.Id);
            ReloadCache();
        }
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
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

    public void ReloadCache()
    {
        _writeInProccess = true;
        Task.Run(() =>
        {
            _cache = Source!.Find(FilterDefinition<T>.Empty).ToList();
            lock (_temp)
            {
                _temp = _cache;
            }
        });
        _writeInProccess = false;
    }

    #endregion
}
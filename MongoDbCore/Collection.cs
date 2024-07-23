namespace MongoDbCore;

public class Collection<T> where T : BaseEntity
{
    #region Initialize

    internal readonly IMongoCollection<T>? Source;
    internal readonly MongoDbContext DbContext;

    private readonly bool _isCacheable;
    private IFindFluent<T, T>? _cache;
    //private List<T> _cacheAsList;
    private List<IncludeReference> _includeReferences = [];

    public Collection(MongoDbContext dbContext)
    {
        DbContext = dbContext;
        Source = dbContext.GetCollection<T>(typeof(T).Name.Pluralize().Underscore());
        _isCacheable = CheckCacheablityOfTEntity();
        //_cacheAsList = IAsyncCursorSourceExtensions.ToList(Source.Find(_ => true));
    }

    #endregion

    #region Queries
    /*public IEnumerator<T> GetEnumerator() => _cacheAsList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public T this[int index]
    {
        get
        {
            // Implement logic to return item at index from your collection
            return _cacheAsList![index];
        }
        set
        {
            // Implement logic to set item at index in your collection
            var existingItem = _cacheAsList![index];
            if (existingItem != null)
            {
                // Update the existing item with the new value
                Update(value);
            }
            else
            {
                // Insert the new item at the specified index
                Add(value);
            }
        }
    }*/

    public List<T> ToList()
    {
        var res = Get().ToList(_includeReferences, DbContext);
        _includeReferences.Clear();
        return res;
    }

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        => Get().ToListAsync(cancellationToken);

    public T? FirstOrDefault()
        => Get().FirstOrDefault(_includeReferences, DbContext);

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
        => Get(predicate).FirstOrDefault(_includeReferences, DbContext);

    public Task<T> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        => Get().FirstOrDefaultAsync(cancellationToken);

    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Get(predicate).FirstOrDefaultAsync(cancellationToken);

    public bool Any()
        => Get().Any();

    public bool Any(Expression<Func<T, bool>> predicate)
        => Get(predicate).Any();

    public List<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        => Get().ToList().Select(selector.Compile()).ToList();

    public List<TResult> SelectMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector)
        => Get().ToList().SelectMany(selector.Compile()).ToList();

    public long Count()
        => Source!.CountDocuments(FilterDefinition<T>.Empty);

    public List<TDto> Get<TDto>()
    {
        return DbContext.GetCollection<TDto>(typeof(T).Name.Pluralize().Underscore()).Find(_=>true).ToList();
    }

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        Source!.InsertOne(entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Source!.InsertOneAsync(entity, null, cancellationToken);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        Source!.InsertMany(entities);
        if (_isCacheable) UpdateCache();
        return entities;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await Source!.InsertManyAsync(entities, null, cancellationToken);
        if (_isCacheable) UpdateCache();
        return entities;
    }

    public T Update(T entity)
    {
        Source!.ReplaceOne(x => x.Id == entity.Id, entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var replaceOptions = new ReplaceOptions();
        await Source!.ReplaceOneAsync(x => x.Id == entity.Id, entity, replaceOptions, cancellationToken);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public void Delete(string id)
    {
        Source!.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, id));
        if (_isCacheable) UpdateCache();
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await Source!.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id), cancellationToken);
        if (_isCacheable) UpdateCache();
    }

    public void Delete(T entity)
        => Delete(entity.Id);

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        => DeleteAsync(entity.Id, cancellationToken);

    #endregion

    #region Extensions

    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        where TProperty : BaseEntity
    {
        var property = CollectionExtensions.ExtractProperty(includeExpression);

        var propertyProperties = typeof(TProperty).GetProperties();
        var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKeyTo>() is not null).ToList();
        if (!foreignKeyProperties.Any())
        {
            throw new Exception("Foreign key attribute is not found.");
        }

        PropertyInfo? foreignKeyProperty = null;

        foreach (var fkProperty in foreignKeyProperties)
        {
            var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => x.NamedArguments is not null && 
                                                                            x.NamedArguments.Any() &&
                                                                            (string)x.NamedArguments[0].TypedValue.Value! == typeof(T).Name);
            if (attribute is null)
            {
                continue;
            }

            foreignKeyProperty = fkProperty;
            break;
        }

        if (foreignKeyProperty == null)
        {
            throw new Exception("Foreign key property is not found.");
        }

        var collectionName = property.PropertyType.Name.Pluralize().Underscore();

        _includeReferences.Add(
            new IncludeReference()
            {
                EqualityProperty = typeof(T).GetProperty("Id")!,
                Order = 1,
                Destination = new()
                {
                    PropertyInfo = property,
                    CollectionName = Source!.CollectionNamespace.CollectionName
                },
                Source = new()
                {
                    CollectionName = collectionName,
                    PropertyInfo = foreignKeyProperty
                }
            });

        return new IncludableQueryable<T, TProperty>(this, _includeReferences);
    }

    public IIncludableQueryable<T, TProperty> IncludeRef<TProperty>(Expression<Func<T, TProperty>> includeExpression) 
        where TProperty : BaseEntity
    {
        var property = CollectionExtensions.ExtractProperty(includeExpression);

        PropertyInfo? refProperty = null;
        string? refPropertyName = null;
        var properties = typeof(T).GetProperties();
        foreach (var propertyInfo in properties)
        {
            var refAttribute = propertyInfo.GetCustomAttribute<ReferenceTo>();
            if (refAttribute is not null && !string.IsNullOrEmpty(refAttribute.Entity))
            {
                refProperty = propertyInfo;
                refPropertyName = refAttribute.Entity;
            }
        }

        var collectionName = typeof(TProperty).Name.Pluralize().Underscore();

        _includeReferences.Add(
            new IncludeReference()
            {
                EqualityProperty = refProperty!,
                Order = 1,
                Destination = new()
                {
                    PropertyInfo = property,
                    CollectionName = Source!.CollectionNamespace.CollectionName
                },
                Source = new()
                {
                    CollectionName = collectionName,
                    PropertyInfo = typeof(TProperty).GetProperty("Id")
                }
            });

        return new IncludableQueryable<T, TProperty>(this, _includeReferences);
    }

    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> includeExpression)
        where TProperty : BaseEntity
    {
        var property = CollectionExtensions.ExtractProperty(includeExpression);

        var propertyProperties = typeof(TProperty).GetProperties();
        var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKeyTo>() is not null).ToList();
        if (!foreignKeyProperties.Any())
        {
            throw new Exception("Foreign key attribute is not found.");
        }

        PropertyInfo? foreignKeyProperty = default;

        foreach (var fkProperty in foreignKeyProperties)
        {
            var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => x.NamedArguments is not null && 
                                                                            x.NamedArguments.Any() &&
                                                                            (string)x.NamedArguments[0].TypedValue.Value! == typeof(T).Name ||
                                                                            x.ConstructorArguments is not null &&
                                                                            x.ConstructorArguments.Any() &&
                                                                            (string)x.ConstructorArguments[0].Value! == typeof(T).Name);
            if (attribute is null)
            {
                continue;
            }

            foreignKeyProperty = fkProperty;
            break;
        }

        if (foreignKeyProperty == null)
        {
            throw new Exception("Foreign key property is not found.3");
        }

        var collectionName = typeof(TProperty).Name.Pluralize().Underscore();

        _includeReferences.Add(
            new IncludeReference()
            {
                EqualityProperty = typeof(T).GetProperty("Id")!,
                Order = 1,
                Destination = new()
                {
                    PropertyInfo = property,
                    CollectionName = Source!.CollectionNamespace.CollectionName
                },
                Source = new()
                {
                    CollectionName = collectionName,
                    PropertyInfo = foreignKeyProperty
                }
            });
        return new IncludableQueryable<T, TProperty>(this, _includeReferences);
    }

    public IFindFluent<T, T> AsFindFluent()
        => Source.Find(_ => true);

    public IFindFluent<T, T> Where(Expression<Func<T, bool>> predicate)
        => Source.Find(predicate);

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
        _cache = Source.Find(FilterDefinition<T>.Empty);
    }

    private IFindFluent<T, T> Get(FilterDefinition<T>? filter = null)
    {
        if (!_isCacheable || _cache == null)
        {
            _cache = Source.Find(filter ?? FilterDefinition<T>.Empty);
            return _cache;
        }

        return _cache;
    }

    #endregion
}
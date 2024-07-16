using System.Collections;
using System.Reflection;

namespace MongoDbCore;

public class Collection<T> : IEnumerable<T> where T : BaseEntity
{
    #region Initialize

    private IMongoCollection<T>? _collection;
    private readonly bool _isCacheable;
    private readonly MongoDbContext dbContext;
    private IFindFluent<T, T>? _cache;

    public Collection(MongoDbContext dbContext)
    {
        _collection = dbContext.GetCollection<T>(typeof(T).Name.Pluralize().Underscore());
        _isCacheable = CheckCacheablityOfTEntity();
        this.dbContext = dbContext;
    }

    #endregion

    #region Queries

    private Dictionary<PropertyInfo, dynamic> includeValues = [];

    public T this[int index]
    {
        get
        {
            // Implement logic to return item at index from your collection
            return ToList()[index];
        }
        set
        {
            // Implement logic to set item at index in your collection
            var existingItem = ToList()[index];
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
    }

    public List<T> ToList() => Get().ToList(includeValues);

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        => Get().ToListAsync(cancellationToken);

    public T FirstOrDefault()
        => Get().FirstOrDefault();

    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        => Get(predicate).FirstOrDefault();

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
        => _collection!.CountDocuments(FilterDefinition<T>.Empty);

    public List<TT> Get<TT>()
    {
        return dbContext.GetCollection<TT>(typeof(T).Name.Pluralize().Underscore()).Find(_=>true).ToList();
    }

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        _collection!.InsertOne(entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _collection!.InsertOneAsync(entity, null, cancellationToken);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        _collection!.InsertMany(entities);
        if (_isCacheable) UpdateCache();
        return entities;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _collection!.InsertManyAsync(entities, null, cancellationToken);
        if (_isCacheable) UpdateCache();
        return entities;
    }

    public T Update(T entity)
    {
        _collection!.ReplaceOne(x => x.Id == entity.Id, entity);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var replaceOptions = new ReplaceOptions();
        await _collection!.ReplaceOneAsync(x => x.Id == entity.Id, entity, replaceOptions, cancellationToken);
        if (_isCacheable) UpdateCache();
        return entity;
    }

    public void Delete(string id)
    {
        _collection!.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, id));
        if (_isCacheable) UpdateCache();
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection!.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id), cancellationToken);
        if (_isCacheable) UpdateCache();
    }

    public void Delete(T entity)
        => Delete(entity.Id);

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        => DeleteAsync(entity.Id, cancellationToken);

    #endregion

    #region Extensions

    public Collection<T> Include<TProperty>(Expression<Func<T, TProperty>> includeExpression) where TProperty : BaseEntity
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
            var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => (string)x.NamedArguments[0].TypedValue.Value! == typeof(T).Name);
            if (attribute is null)
            {
                throw new Exception("Foreign key attribute is not found.2");
            }

            foreignKeyProperty = fkProperty;
            break;
        }

        if (foreignKeyProperty == null)
        {
            throw new Exception("Foreign key property is not found.3");
        }

        var foreignKeyPropertyName = foreignKeyProperty.Name;

        for (int i = 0; i < Count(); i++)
        {
            var item = this[i];

            var filter = Builders<TProperty>.Filter.Eq(foreignKeyPropertyName, item.Id);

            var collectionName = property.PropertyType.Name.Pluralize().Underscore();
            var collection = dbContext.GetCollection<TProperty>(collectionName);

            // Create a projection to include the necessary fields
            var projection = Builders<TProperty>.Projection.Include(foreignKeyPropertyName);

            var foreignPropertyValue = IAsyncCursorSourceExtensions.FirstOrDefault(collection.Find(filter).Project<TProperty>(projection));

            if (foreignPropertyValue != null)
            {
                includeValues.Add(property, foreignPropertyValue);
            }
        }

        return this;
    }

    public Collection<T> IncludeRef<TProperty>(Expression<Func<T, TProperty>> includeExpression) where TProperty : BaseEntity
    {
        var property = CollectionExtensions.ExtractProperty(includeExpression);

        var refAttribute = property.GetCustomAttribute<ReferenceTo>();
        if (refAttribute is null)
        { 
            throw new Exception("Reference To attribute is not found.");
        }

        if (string.IsNullOrEmpty(refAttribute.Entity))
        {
            throw new Exception("Entity name is not found.");
        }

        for (int i = 0; i < Count(); i++)
        {
            var item = this[i];
            var obj = property.GetValue(item);
            if (obj is not null)
            {
                var value = (TProperty)obj;
                var filter = Builders<TProperty>.Filter.Eq("Id", (value).Id);

                var collectionName = property.PropertyType.Name.Pluralize().Underscore();
                var collection = dbContext.GetCollection<TProperty>(collectionName);

                var foreignPropertyValue = IAsyncCursorSourceExtensions.FirstOrDefault(collection.Find(filter));

                if (foreignPropertyValue != null)
                {
                    includeValues.Add(property, foreignPropertyValue);
                }
            }
        }

        return this;
    }

    public Collection<T> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> includeExpression) where TProperty : BaseEntity
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
            var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => (string)x.NamedArguments[0].TypedValue.Value! == typeof(T).Name);
            if (attribute is null)
            {
                throw new Exception("Foreign key attribute is not found.2");
            }

            foreignKeyProperty = fkProperty;
            break;
        }

        if (foreignKeyProperty == null)
        {
            throw new Exception("Foreign key property is not found.3");
        }

        var foreignKeyPropertyName = foreignKeyProperty.Name;

        for (int i = 0; i < Count(); i++)
        {
            var item = this[i];

            var filter = Builders<TProperty>.Filter.Eq(foreignKeyPropertyName, item.Id);

            var collectionName = typeof(TProperty).Name.Pluralize().Underscore();
            var collection = dbContext.GetCollection<TProperty>(collectionName);

            // Create a projection to include the necessary fields
            var projection = Builders<TProperty>.Projection.Include(foreignKeyPropertyName);

            var foreignPropertyValue = IAsyncCursorSourceExtensions.ToList(collection.Find(filter).Project<TProperty>(projection));

            if (foreignPropertyValue != null)
            {
                includeValues.TryAdd(property, foreignPropertyValue);
            }
        }

        return this;
    }

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
        if (!_isCacheable || _cache == null)
        {
            _cache = _collection.Find(filter ?? FilterDefinition<T>.Empty);
        }

        var list = _cache.ToList();
        return _cache;
    }

    #endregion

    #region IEnumerable<T> Implementation

    public IEnumerator<T> GetEnumerator()
    {
        return ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}
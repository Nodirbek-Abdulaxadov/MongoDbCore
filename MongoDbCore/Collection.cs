using System.Reflection;

namespace MongoDbCore;

public class Collection<T> where T : BaseEntity
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

    public List<T> ToList()
        => Get().ToList();

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

    public IFindFluent<T, T> Include(Expression<Func<T, object>> includeExpression)
    {
        // Extract the property name from the expressionstring
        var  propertyName = GetPropertyName(includeExpression);

        // Perform additional query to fetch related documents or fields
        var projection = Builders<T>.Projection.Include(propertyName);

        var result = _collection.Find(_ => true).Project<T>(projection);

        return result;
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

    private string GetPropertyName(Expression<Func<T, object>> propertyExpression)
    {
        var memberExpression = propertyExpression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new ArgumentException("Expression is not a member access expression.", nameof(propertyExpression));
        }

        return memberExpression.Member.Name;
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
}
using System.Text.Json;

namespace MongoDbCore;

/// <summary>
/// Thin wrapper around <see cref="IMongoCollection{T}"/> that adds:
/// <list type="bullet">
///   <item>conventional naming (pluralized snake_case)</item>
///   <item>id management and timestamp maintenance</item>
///   <item>optional auditing for entities decorated with <see cref="Auditable"/></item>
///   <item><c>Include</c>/<c>IncludeRef</c> eager-loading helpers</item>
/// </list>
/// </summary>
public class Collection<T> where T : BaseEntity
{
    #region Initialize

    public readonly IMongoCollection<T> Source;
    public readonly MongoDbContext DbContext;
    public readonly string CollectionName;

    private readonly bool _isAuditable;
    private readonly IAuditService? _auditService;
    private readonly List<IncludeReference> _includeReferences = [];

    public Collection(MongoDbContext dbContext, IAuditService? auditService = null)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        CollectionName = typeof(T).Name.Pluralize().Underscore();
        Source = dbContext.GetCollection<T>(CollectionName);
        _isAuditable = typeof(T).GetCustomAttributes(typeof(Auditable), true).Length > 0;
        _auditService = auditService;
    }

    #endregion

    #region Queries

    public List<T> ToList()
    {
        var find = Get();
        if (_includeReferences.Count > 0)
        {
            var res = find.ToList(_includeReferences, DbContext);
            _includeReferences.Clear();
            return res;
        }
        return find.ToList();
    }

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        => Get().ToListAsync(cancellationToken);

    public T? FirstOrDefault()
        => Get().FirstOrDefault(_includeReferences, DbContext);

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
        => Get(predicate).FirstOrDefault(_includeReferences, DbContext);

    public Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        => Get().FirstOrDefaultAsync(cancellationToken)!;

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Get(predicate).FirstOrDefaultAsync(cancellationToken)!;

    public bool Any() => Get().Any();

    public bool Any(Expression<Func<T, bool>> predicate) => Get(predicate).Any();

    /// <summary>
    /// Server-side projection. Note: the selector must be translatable by the
    /// MongoDB LINQ provider; non-translatable expressions throw.
    /// </summary>
    public List<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        => Source.AsQueryable().Select(selector).ToList();

    public List<TResult> SelectMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector)
        => Source.AsQueryable().SelectMany(selector).ToList();

    public long Count() => Source.CountDocuments(FilterDefinition<T>.Empty);

    public Task<long> CountAsync(CancellationToken cancellationToken = default)
        => Source.CountDocumentsAsync(FilterDefinition<T>.Empty, cancellationToken: cancellationToken);

    public long Count(Expression<Func<T, bool>> predicate)
        => Source.CountDocuments(predicate);

    public Task<long> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Source.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);

    public List<TDto> Get<TDto>()
        => DbContext.GetCollection<TDto>(CollectionName).Find(_ => true).ToList();

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        EnsureValidId(entity);
        TouchTimestamps(entity, isUpdate: false);
        Source.InsertOne(entity);
        CheckAudit(ActionType.Add, null, entity);
        return entity;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        EnsureValidId(entity);
        TouchTimestamps(entity, isUpdate: false);
        await Source.InsertOneAsync(entity, null, cancellationToken);
        CheckAudit(ActionType.Add, null, entity);
        return entity;
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        var list = entities as IList<T> ?? entities.ToList();
        foreach (var e in list)
        {
            EnsureValidId(e);
            TouchTimestamps(e, isUpdate: false);
        }
        Source.InsertMany(list);
        CheckAudit(ActionType.AddRange, list, null);
        return list;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var list = entities as IList<T> ?? entities.ToList();
        foreach (var e in list)
        {
            EnsureValidId(e);
            TouchTimestamps(e, isUpdate: false);
        }
        await Source.InsertManyAsync(list, null, cancellationToken);
        CheckAudit(ActionType.AddRange, list, null);
        return list;
    }

    public T Update(T entity)
    {
        TouchTimestamps(entity, isUpdate: true);
        CheckAudit(ActionType.Update, entity.Id, entity);
        Source.ReplaceOne(x => x.Id == entity.Id, entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        TouchTimestamps(entity, isUpdate: true);
        CheckAudit(ActionType.Update, entity.Id, entity);
        await Source.ReplaceOneAsync(x => x.Id == entity.Id, entity, new ReplaceOptions(), cancellationToken);
        return entity;
    }

    public long UpdateMany<TProperty>(Expression<Func<T, bool>> filter,
                                      Expression<Func<T, TProperty>> propertySelection,
                                      TProperty newValue)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(propertySelection);

        var update = Builders<T>.Update
            .Set(propertySelection, newValue)
            .Set(t => t.UpdatedAt, DateTimeOffset.UtcNow);

        return Source.UpdateMany(filter, update).ModifiedCount;
    }

    public async Task<long> UpdateManyAsync<TProperty>(Expression<Func<T, bool>> filter,
                                                      Expression<Func<T, TProperty>> propertySelection,
                                                      TProperty newValue,
                                                      CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(propertySelection);

        var update = Builders<T>.Update
            .Set(propertySelection, newValue)
            .Set(t => t.UpdatedAt, DateTimeOffset.UtcNow);

        var result = await Source.UpdateManyAsync(filter, update, null, cancellationToken);
        return result.ModifiedCount;
    }

    public void Delete(string id)
    {
        CheckAudit(ActionType.Delete, id, null);
        Source.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, id));
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        CheckAudit(ActionType.Delete, id, null);
        return Source.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id), cancellationToken);
    }

    public void Delete(T entity) => Delete(entity.Id);

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        => DeleteAsync(entity.Id, cancellationToken);

    public void DeleteRange(IEnumerable<T> entities)
    {
        var ids = entities.Select(x => x.Id).ToList();
        Source.DeleteMany(Builders<T>.Filter.In(x => x.Id, ids));
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var ids = entities.Select(x => x.Id).ToList();
        return Source.DeleteManyAsync(Builders<T>.Filter.In(x => x.Id, ids), cancellationToken);
    }

    public void DeleteMany(Expression<Func<T, bool>> filter) => Source.DeleteMany(filter);

    public Task DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
        => Source.DeleteManyAsync(filter, cancellationToken);

    public void DeleteAll() => Source.DeleteMany(FilterDefinition<T>.Empty);

    public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        => Source.DeleteManyAsync(FilterDefinition<T>.Empty, cancellationToken);

    #endregion

    #region Includes

    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        where TProperty : BaseEntity
    {
        var property = CollectionExtensions.ExtractProperty(includeExpression);
        var foreignKeyProperty = FindForeignKey<TProperty>(typeof(T));

        if (foreignKeyProperty == null)
            return new IncludableQueryable<T, TProperty>(this, _includeReferences);

        _includeReferences.Add(new IncludeReference
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = Source.CollectionNamespace.CollectionName
            },
            Source = new()
            {
                CollectionName = property.PropertyType.Name.Pluralize().Underscore(),
                PropertyInfo = foreignKeyProperty
            }
        });

        return new IncludableQueryable<T, TProperty>(this, _includeReferences);
    }

    public IIncludableQueryable<T, TProperty> IncludeRef<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        where TProperty : BaseEntity
    {
        var property = CollectionExtensions.ExtractProperty(includeExpression);
        var refProperty = typeof(T).GetProperties()
            .FirstOrDefault(p =>
            {
                var attr = p.GetCustomAttribute<ReferenceTo>();
                return attr is not null && !string.IsNullOrEmpty(attr.Entity);
            });

        _includeReferences.Add(new IncludeReference
        {
            EqualityProperty = refProperty!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = Source.CollectionNamespace.CollectionName
            },
            Source = new()
            {
                CollectionName = typeof(TProperty).Name.Pluralize().Underscore(),
                PropertyInfo = typeof(TProperty).GetProperty("Id")
            }
        });

        return new IncludableQueryable<T, TProperty>(this, _includeReferences);
    }

    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> includeExpression)
        where TProperty : BaseEntity
    {
        var property = CollectionExtensions.ExtractProperty(includeExpression);
        var foreignKeyProperty = FindForeignKey<TProperty>(typeof(T))
            ?? throw new InvalidOperationException($"Foreign key not found on {typeof(TProperty).Name} pointing back to {typeof(T).Name}.");

        _includeReferences.Add(new IncludeReference
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = Source.CollectionNamespace.CollectionName
            },
            Source = new()
            {
                CollectionName = typeof(TProperty).Name.Pluralize().Underscore(),
                PropertyInfo = foreignKeyProperty
            }
        });
        return new IncludableQueryable<T, TProperty>(this, _includeReferences);
    }

    public IFindFluent<T, T> AsFindFluent() => Source.Find(_ => true);

    public IFindFluent<T, T> Where(Expression<Func<T, bool>> predicate) => Source.Find(predicate);

    public IFindFluent<T, T> Where(FilterDefinition<T> filter) => Source.Find(filter);

    #endregion

    #region Helpers

    private static PropertyInfo? FindForeignKey<TProperty>(Type ownerType)
    {
        foreach (var fk in typeof(TProperty).GetProperties())
        {
            var attr = fk.GetCustomAttribute<ForeignKeyTo>();
            if (attr is null) continue;
            if (string.Equals(attr.Entity, ownerType.Name, StringComparison.Ordinal))
                return fk;
        }
        return null;
    }

    private static void EnsureValidId(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id) || !ObjectId.TryParse(entity.Id, out _))
            entity.Id = BaseEntity.NewId;
    }

    private static void TouchTimestamps(T entity, bool isUpdate)
    {
        var now = DateTimeOffset.UtcNow;
        if (!isUpdate)
            entity.CreatedAt = now;
        entity.UpdatedAt = now;
    }

    private IFindFluent<T, T> Get(FilterDefinition<T>? filter = null)
        => Source.Find(filter ?? FilterDefinition<T>.Empty);

    #endregion

    #region Audit

    private void CheckAudit(ActionType actionType, string? oldId, T? entity)
    {
        if (!_isAuditable) return;

        if (_auditService is null)
            throw new InvalidOperationException("IAuditService was not registered. Enable auditing via MongoDbCoreOptions.EnableAuditing.");

        string oldValue = string.Empty;
        string newValue = string.Empty;

        if (!string.IsNullOrEmpty(oldId))
        {
            var oldEntity = FirstOrDefault(x => x.Id == oldId);
            if (oldEntity is not null) oldValue = JsonSerializer.Serialize(oldEntity);
        }
        if (entity is not null) newValue = JsonSerializer.Serialize(entity);

        _auditService.Add(new AuditEntity
        {
            ActionType = actionType,
            Collection = CollectionName,
            OldValue = oldValue,
            NewValue = newValue
        });
    }

    private void CheckAudit(ActionType actionType, IEnumerable<T> entities, string? oldId)
    {
        if (!_isAuditable) return;

        if (_auditService is null)
            throw new InvalidOperationException("IAuditService was not registered. Enable auditing via MongoDbCoreOptions.EnableAuditing.");

        string oldValue = string.Empty;
        string newValue = string.Empty;

        if (!string.IsNullOrEmpty(oldId))
        {
            var oldEntity = FirstOrDefault(x => x.Id == oldId);
            if (oldEntity is not null) oldValue = JsonSerializer.Serialize(oldEntity);
        }
        if (entities.Any()) newValue = JsonSerializer.Serialize(entities);

        _auditService.Add(new AuditEntity
        {
            ActionType = actionType,
            Collection = CollectionName,
            OldValue = oldValue,
            NewValue = newValue
        });
    }

    #endregion
}

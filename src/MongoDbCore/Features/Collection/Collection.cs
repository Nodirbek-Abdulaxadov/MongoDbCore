using SharpCompress.Common;
using System.Text.Json;

public class Collection<T> where T : BaseEntity
{
    #region Initialize

    public readonly IMongoCollection<T> Source;
    public readonly MongoDbContext DbContext;
    public readonly string CollectionName = string.Empty;

    private readonly bool _isAuditable;
    private IAuditService? _auditService;
    private List<IncludeReference> _includeReferences = [];

    public Collection(MongoDbContext dbContext, IAuditService? auditService)
    {
        DbContext = dbContext;
        CollectionName = typeof(T).Name.Pluralize().Underscore();
        Source = dbContext.GetCollection<T>(CollectionName);
        _isAuditable = CheckAuditableOfTEntity();
        if (auditService is not null)
        {
            _auditService = auditService;
        }
    }

    #endregion

    #region Queries
    public List<T> ToList()
    {
        var findResults = Get();
        if (_includeReferences.Any())
        {
            var res = findResults.ToList(_includeReferences, DbContext);
            _includeReferences.Clear();
            return res;
        }

        return findResults.ToList();
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

    public Task<long> CountAsync()
        => Source!.CountDocumentsAsync(FilterDefinition<T>.Empty);

    public List<TDto> Get<TDto>()
    {
        return DbContext.GetCollection<TDto>(typeof(T).Name.Pluralize().Underscore()).Find(_=>true).ToList();
    }

    #endregion

    #region Mutation

    public T Add(T entity)
    {
        if (ObjectId.TryParse(entity.Id, out var id))
        {
            entity.Id = BaseEntity.NewId;
        }
        Source!.InsertOne(entity);
        CheckAudit(ActionType.Add, null, entity);
        return entity;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = BaseEntity.NewId;
        }
        await Source!.InsertOneAsync(entity, null, cancellationToken);
        CheckAudit(ActionType.Add, null, entity);
        return entity;
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        Source!.InsertMany(entities);
        CheckAudit(ActionType.AddRange, entities, null);
        return entities;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await Source!.InsertManyAsync(entities, null, cancellationToken);
        CheckAudit(ActionType.AddRange, entities, null);
        return entities;
    }

    public T Update(T entity)
    {
        entity.UpdatedAt = DateTime.Now;
        CheckAudit(ActionType.Update, entity.Id, entity);
        Source!.ReplaceOne(x => x.Id == entity.Id, entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.Now;
        var replaceOptions = new ReplaceOptions();
        CheckAudit(ActionType.Update, entity.Id, entity);
        await Source!.ReplaceOneAsync(x => x.Id == entity.Id, entity, replaceOptions, cancellationToken);
        return entity;
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
            var update = Builders<T>.Update.Set(propertySelection, newValue).Set(t => t.UpdatedAt, DateTime.Now);
            var result = Source.UpdateMany(filter, update);

            return result.ModifiedCount;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while updating documents.", ex);
        }
    }

    public async Task<long> UpdateManyAsync<TProperty>(Expression<Func<T, bool>> filter,
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

        try
        {
            var update = Builders<T>.Update.Set(propertySelection, newValue);
            var result = await Source.UpdateManyAsync(filter, update, null, cancellationToken);

            return result.ModifiedCount;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while updating documents asynchronously.", ex);
        }
    }

    public void Delete(string id)
    {
        CheckAudit(ActionType.Delete, id, null);
        Source!.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, id));
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        CheckAudit(ActionType.Delete, id, null);
        await Source!.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id), cancellationToken);
    }

    public void Delete(T entity)
    {
        CheckAudit(ActionType.Delete, entity.Id, null);
        Delete(entity.Id);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        CheckAudit(ActionType.Delete, entity.Id, null);
        return DeleteAsync(entity.Id, cancellationToken);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        var ids = entities.Select(x => x.Id).ToList();
        Source!.DeleteMany(Builders<T>.Filter.In(x => x.Id, ids));
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var ids = entities.Select(x => x.Id).ToList();
        await Source!.DeleteManyAsync(Builders<T>.Filter.In(x => x.Id, ids), cancellationToken);
    }

    public void DeleteMany(Expression<Func<T, bool>> filter)
        => Source!.DeleteMany(filter);

    public async Task DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
        => await Source!.DeleteManyAsync(filter, cancellationToken);

    public void DeleteAll()
    {
        Source!.DeleteMany(FilterDefinition<T>.Empty);
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        await Source!.DeleteManyAsync(FilterDefinition<T>.Empty, cancellationToken);
    }

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
            return new IncludableQueryable<T, TProperty>(this, _includeReferences);
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

    public IFindFluent<T, T> Where(FilterDefinition<T> filter)
        => Source.Find(filter);

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

    private bool CheckAuditableOfTEntity()
        => typeof(T).GetCustomAttributes(typeof(Auditable), true).Any();

    private IFindFluent<T, T> Get(FilterDefinition<T>? filter = null)
    {
        StackTrace stackTrace = new StackTrace();
        string callers = string.Join(", ", stackTrace.GetFrames()!.Take(3).Select(x => x.GetMethod()!.Name));

#if DEBUG
        Console.WriteLine($"Call: {typeof(T).Name}, Called from: {callers}");
#endif

        return Source.Find(filter ?? FilterDefinition<T>.Empty);
    }

    #endregion

    #region Check Audit

    private void CheckAudit(ActionType actionType, string? oldId = null, T? entity = null)
    {
        if (_isAuditable)
        {
            if (_auditService is null)
            {
                throw new InvalidOperationException("IAuditService was not registered in the DI container.");
            }

            string oldValue = string.Empty;
            string newValue = string.Empty;

            if (!string.IsNullOrEmpty(oldId))
            {
                var oldEntity = FirstOrDefault(x => x.Id == oldId);
                if (oldEntity is not null)
                {
                    oldValue = JsonSerializer.Serialize(oldEntity);
                }
            }

            if (entity is not null)
            {
                newValue = JsonSerializer.Serialize(entity);
            }


            AuditEntity auditEntity = new()
            {
                ActionType = actionType,
                Collection = CollectionName,
                OldValue = oldValue,
                NewValue = newValue
            };

            _auditService.Add(auditEntity);
        }
    }

    private void CheckAudit(ActionType actionType, IEnumerable<T> entities, string? oldId = null)
    {
        if (_isAuditable)
        {
            if (_auditService is null)
            {
                throw new InvalidOperationException("IAuditService was not registered in the DI container.");
            }

            string oldValue = string.Empty;
            string newValue = string.Empty;

            if (!string.IsNullOrEmpty(oldId))
            {
                var oldEntity = FirstOrDefault(x => x.Id == oldId);
                if (oldEntity is not null)
                {
                    oldValue = JsonSerializer.Serialize(oldEntity);
                }
            }

            if (entities.Any())
            {
                newValue = JsonSerializer.Serialize(entities);
            }


            AuditEntity auditEntity = new()
            {
                ActionType = actionType,
                Collection = CollectionName,
                OldValue = oldValue,
                NewValue = newValue
            };

            _auditService.Add(auditEntity);
        }
    }

    #endregion
}
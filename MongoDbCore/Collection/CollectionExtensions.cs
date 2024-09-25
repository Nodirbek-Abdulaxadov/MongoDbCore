using System.Collections.Concurrent;

namespace MongoDbCore;

public static class CollectionExtensions
{
    #region Sorting and Filtering
    public static IFindFluent<T, T> OrderBy<T>(this IFindFluent<T, T> findFluent, Expression<Func<T, object>> expression)
        => findFluent.SortBy(expression);

    public static IFindFluent<T, T> OrderByDescending<T>(this IFindFluent<T, T> findFluent, Expression<Func<T, object>> expression)
        => findFluent.SortByDescending(expression);

    public static List<T> Take<T>(this IFindFluent<T, T> findFluent, int count)
        => findFluent.Limit(count).ToList();
    #endregion

    #region ToList

    public static Task<List<T>> ToListAsync<T>(this IFindFluent<T, T> findFluent, CancellationToken cancellationToken = default)
        => IAsyncCursorSourceExtensions.ToListAsync(findFluent, cancellationToken);

    public static List<T> ToList<T>(this IFindFluent<T, T> findFluent)
        where T : BaseEntity
        => IAsyncCursorSourceExtensions.ToList(findFluent);

    #region Sync
    public static List<T> ToList<T, TDbContext>(this IFindFluent<T, T> findFluent, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default)
    where T : BaseEntity
    where TDbContext : MongoDbContext
    => ToListFromIAsyncCursorSource(findFluent, includeReferences, dbContext);

    private static List<TDocument> ToListFromIAsyncCursorSource<TDocument, TDbContext>(this IAsyncCursorSource<TDocument> source, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        using var cursor = source.ToCursor(cancellationToken);
        return cursor.ToListFromIAsyncCursor(includeReferences, dbContext, cancellationToken);
    }

    private static List<TDocument> ToListFromIAsyncCursor<TDocument, TDbContext>(this IAsyncCursor<TDocument> cursor, List<IncludeReference>? includeReferences = null, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        var concurrentList = new ConcurrentBag<TDocument>();

        while (cursor.MoveNext(cancellationToken))
        {
            foreach (var item in cursor.Current)
            {
                concurrentList.Add(item);
            }
        }

        if (includeReferences is not null && includeReferences.Any())
        {
            Parallel.ForEach(concurrentList, item =>
            {
                var processedItem = SetReferences(item, includeReferences, dbContext);
                if (processedItem != null)
                {
                    concurrentList.Add(processedItem); // Thread-safe collection
                }
            });
        }

        return concurrentList.ToList(); // Convert ConcurrentBag to List before returning
    } 
    #endregion

    #region Async
    public static Task<List<T>> ToListAsync<T, TDbContext>(this IFindFluent<T, T> findFluent, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default)
    where T : BaseEntity
    where TDbContext : MongoDbContext
    => ToListFromIAsyncCursorSourceAsync(findFluent, includeReferences, dbContext);

    private static Task<List<TDocument>> ToListFromIAsyncCursorSourceAsync<TDocument, TDbContext>(this IAsyncCursorSource<TDocument> source, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        using var cursor = source.ToCursor(cancellationToken);
        return cursor.ToListFromIAsyncCursorAsync(includeReferences, dbContext, cancellationToken);
    }

    private static async Task<List<TDocument>> ToListFromIAsyncCursorAsync<TDocument, TDbContext>(this IAsyncCursor<TDocument> cursor, List<IncludeReference>? includeReferences = null, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        var concurrentList = new ConcurrentBag<TDocument>();

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var item in cursor.Current)
            {
                concurrentList.Add(item);
            }
        }

        if (includeReferences is not null && includeReferences.Any())
        {
            Parallel.ForEach(concurrentList, item =>
            {
                var processedItem = SetReferences(item, includeReferences, dbContext);
                if (processedItem != null)
                {
                    concurrentList.Add(processedItem); // Thread-safe collection
                }
            });
        }

        return concurrentList.ToList(); // Convert ConcurrentBag to List before returning
    } 
    #endregion

    #endregion

    #region FirstOrDefault without expression
    public static Task<TProjection> FirstOrDefaultAsync<TDocument, TProjection>(this IFindFluent<TDocument, TProjection> find, CancellationToken cancellationToken = default)
        where TProjection : BaseEntity
        => IAsyncCursorSourceExtensions.FirstOrDefaultAsync(find, cancellationToken);

    public static TProjection? FirstOrDefault<TDocument, TProjection>(this IFindFluent<TDocument, TProjection> find)
        where TProjection : BaseEntity
        => IAsyncCursorSourceExtensions.FirstOrDefault(find);

    public static Task<TProjection?> FirstOrDefaultAsync<TDocument, TProjection, TDbContext>(this IFindFluent<TDocument, TProjection> find, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TProjection : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        return Task.FromResult(find.Limit(1).FirstOrDefault2(includeReferences, dbContext, cancellationToken));
    }

    public static TProjection? FirstOrDefault<TDocument, TProjection, TDbContext>(this IFindFluent<TDocument, TProjection> find, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default)
        where TProjection : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        return find.Limit(1).FirstOrDefault2(includeReferences, dbContext);
    }

    private static TDocument? FirstOrDefault2<TDocument, TDbContext>(this IAsyncCursorSource<TDocument> source, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        TDocument? document;

        if (source is IQueryable<TDocument> queryable && !cancellationToken.CanBeCanceled)
        {
            document = Queryable.FirstOrDefault(queryable)!;
        }
        else
        {
            using var cursor = source.ToCursor(cancellationToken);
            document = cursor.FirstOrDefault(cancellationToken);
        }

        return SetReferences(document, includeReferences, dbContext);
    }

    private static TDocument FirstOrDefault<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default)
    {
        using (cursor)
        {
            var batch = GetFirstBatch(cursor, cancellationToken);
            return batch.FirstOrDefault()!;
        }
    }

    private static IEnumerable<TDocument> GetFirstBatch<TDocument>(IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken)
        => cursor.MoveNext(cancellationToken) ? cursor.Current : Enumerable.Empty<TDocument>();
    #endregion

    #region FirstOrDefault with expression
    public static Task<TDocument> FirstOrDefaultAsync<TDocument>(this IFindFluent<TDocument, TDocument> find, Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
    {
        find.Filter = filter;
        return IAsyncCursorSourceExtensions.FirstOrDefaultAsync(find, cancellationToken);
    }

    public static Task<TDocument?> FirstOrDefaultAsync<TDocument, TDbContext>(this IFindFluent<TDocument, TDocument> find, Expression<Func<TDocument, bool>> filter, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        find.Filter = filter;
        return Task.FromResult(find.Limit(1).FirstOrDefault2(includeReferences, dbContext, cancellationToken));
    }
    
    public static TDocument? FirstOrDefault<TDocument>(this IFindFluent<TDocument, TDocument> find, Expression<Func<TDocument, bool>> filter)
        where TDocument : BaseEntity
    {
        find.Filter = filter;
        return IAsyncCursorSourceExtensions.FirstOrDefault(find);
    }

    public static TDocument? FirstOrDefault<TDocument, TDbContext>(this IFindFluent<TDocument, TDocument> find, Expression<Func<TDocument, bool>> filter, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        find.Filter = filter;
        return find.Limit(1).FirstOrDefault2(includeReferences, dbContext);
    }
    #endregion

    #region Include

    public static IIncludableQueryable<T, TProperty> Include<T, TProperty>(this IFindFluent<T, T> findFluent, Expression<Func<T, TProperty>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var property = ExtractProperty(includeExpression);

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
            throw new Exception("Foreign key property is not found.");
        }

        var collectionName = property.PropertyType.Name.Pluralize().Underscore();
        var dbContext = StaticServiceLocator.DbContext;
        var sourceCollectionName = typeof(T).Name.Pluralize().Underscore();

        var reference = new IncludeReference()
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = sourceCollectionName
            },
            Source = new()
            {
                CollectionName = collectionName,
                PropertyInfo = foreignKeyProperty
            }
        };

        return new IncludableQueryable<T, TProperty>(new Collection<T>(dbContext!), [reference]);
    }

    public static IIncludableQueryable<T, TProperty> IncludeRef<T, TProperty>(this IFindFluent<T, T> findFluent, Expression<Func<T, TProperty>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var property = ExtractProperty(includeExpression);

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

        var dbContext = StaticServiceLocator.DbContext;
        var sourceCollectionName = typeof(T).Name.Pluralize().Underscore();
        var collectionName = typeof(TProperty).Name.Pluralize().Underscore();

        var reference = new IncludeReference()
        {
            EqualityProperty = refProperty!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = sourceCollectionName
            },
            Source = new()
            {
                CollectionName = collectionName,
                PropertyInfo = typeof(TProperty).GetProperty("Id")
            }
        };

        return new IncludableQueryable<T, TProperty>(new Collection<T>(dbContext!), [reference]);
    }

    public static IIncludableQueryable<T, TProperty> Include<T, TProperty>(this IFindFluent<T, T> findFluent, Expression<Func<T, IEnumerable<TProperty>>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var property = ExtractProperty(includeExpression);

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
                throw new Exception("Foreign key attribute is not found.2");
            }

            foreignKeyProperty = fkProperty;
            break;
        }

        if (foreignKeyProperty == null)
        {
            throw new Exception("Foreign key property is not found.3");
        }

        var collectionName = typeof(TProperty).Name.Pluralize().Underscore();
        var dbContext = StaticServiceLocator.DbContext;
        var sourceCollectionName = typeof(T).Name.Pluralize().Underscore();

        var reference = new IncludeReference()
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = sourceCollectionName
            },
            Source = new()
            {
                CollectionName = collectionName,
                PropertyInfo = foreignKeyProperty
            }
        };

        return new IncludableQueryable<T, TProperty>(new Collection<T>(dbContext!), [reference], findFluent.Filter);
    }
    #endregion

    #region Helpers
    internal static PropertyInfo ExtractProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var body = propertyExpression.Body as MemberExpression ?? (propertyExpression.Body as UnaryExpression)?.Operand as MemberExpression;
        if (body?.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Expression does not refer to a property.", nameof(propertyExpression));
        }
        return propertyInfo;
    }

    private static TDocument? SetReferences<TDocument, TDbContext>(TDocument item, List<IncludeReference>? includeReferences, TDbContext? dbContext)
    where TDocument : BaseEntity
    where TDbContext : MongoDbContext
    {
        if (item == null || includeReferences == null || dbContext == null) return item;

        var primaryReferences = includeReferences.Where(x => x.Order == 1).ToList();
        foreach (var reference in primaryReferences)
        {
            if (reference.Source?.PropertyInfo == null || reference.Destination?.PropertyInfo == null) continue;

            var collection = dbContext.GetCollection<BsonDocument>(reference.Source.CollectionName!);
            if (collection == null) continue;

            var equalityValue = (string?)reference.EqualityProperty.GetValue(item);
            if (string.IsNullOrEmpty(equalityValue)) continue;

            var equalityPropertyName = reference.Source.PropertyInfo.Name;
            if (equalityPropertyName == "Id")
            {
                equalityPropertyName = "_id";
            }

            var filter = equalityPropertyName == "_id"
                ? Builders<BsonDocument>.Filter.Eq(equalityPropertyName, ObjectId.Parse(equalityValue))
                : Builders<BsonDocument>.Filter.Eq(equalityPropertyName, equalityValue);

            var sourceValues = collection.Find(filter).ToList();

            if (sourceValues == null || !sourceValues.Any()) continue;

            dynamic deserializedValue = DeserializeValue(reference, sourceValues, includeReferences, dbContext);

            reference.Destination.PropertyInfo.SetValue(item, deserializedValue);
        }

        return item;
    }

    private static dynamic DeserializeValue(IncludeReference reference, List<BsonDocument> sourceValues, List<IncludeReference> includeReferences, MongoDbContext dbContext)
    {
        if (typeof(IEnumerable).IsAssignableFrom(reference.Destination!.PropertyInfo!.PropertyType))
        {
            var itemTypeOfCollection = reference.Destination.PropertyInfo.PropertyType.GetGenericArguments()[0];
            var collectionValues = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemTypeOfCollection))!;

            foreach (var value in sourceValues)
            {
                var deserializedItem = BsonSerializer.Deserialize(value, itemTypeOfCollection);

                SetNestedReferences(deserializedItem, includeReferences, dbContext);
                collectionValues.Add(deserializedItem);
            }
            return collectionValues;
        }
        else
        {
            var deserializedValue = BsonSerializer.Deserialize(sourceValues.First(), reference.Destination.PropertyInfo.PropertyType);
            SetNestedReferences(deserializedValue, includeReferences, dbContext);
            return deserializedValue;
        }
    }

    private static void SetNestedReferences(object deserializedItem, List<IncludeReference> includeReferences, MongoDbContext dbContext)
    {
        var nestedReferences = includeReferences.Where(x => x.Order == 2).ToList();
        foreach (var includeReference in nestedReferences)
        {
            if (includeReference.Source?.PropertyInfo == null || includeReference.Destination?.PropertyInfo == null) continue;

            var collection = dbContext.GetCollection<BsonDocument>(includeReference.Source.CollectionName!);
            if (collection == null) continue;

            var filter = Builders<BsonDocument>.Filter.Eq(includeReference.Source.PropertyInfo.Name, ((BaseEntity)deserializedItem).Id);
            var sourceValue = collection.Find(filter).FirstOrDefault();

            if (sourceValue == null) continue;

            var deserializedValue = BsonSerializer.Deserialize(sourceValue, includeReference.Destination.PropertyInfo.PropertyType);
            includeReference.Destination.PropertyInfo.SetValue(deserializedItem, deserializedValue);
        }
    }

    #endregion
}
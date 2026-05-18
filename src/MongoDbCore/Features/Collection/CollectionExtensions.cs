namespace MongoDbCore;

/// <summary>
/// Extension methods that add convenience operations to
/// <see cref="IFindFluent{TDocument,TProjection}"/> and surface eager-loading
/// (<c>Include</c>) for query chains.
/// </summary>
public static class CollectionExtensions
{
    #region Sorting and Filtering

    public static IFindFluent<T, T> OrderBy<T>(this IFindFluent<T, T> findFluent, Expression<Func<T, object>> expression)
        => findFluent.SortBy(expression);

    public static IFindFluent<T, T> OrderByDescending<T>(this IFindFluent<T, T> findFluent, Expression<Func<T, object>> expression)
        => findFluent.SortByDescending(expression);

    public static List<T> Take<T>(this IFindFluent<T, T> findFluent, int count)
        => findFluent.Limit(count).ToList();

    public static Task<List<T>> TakeAsync<T>(this IFindFluent<T, T> findFluent, int count, CancellationToken cancellationToken = default)
        => findFluent.Limit(count).ToListAsync(cancellationToken);

    #endregion

    #region ToList

    public static Task<List<T>> ToListAsync<T>(this IFindFluent<T, T> findFluent, CancellationToken cancellationToken = default)
        => IAsyncCursorSourceExtensions.ToListAsync(findFluent, cancellationToken);

    public static List<T> ToList<T>(this IFindFluent<T, T> findFluent) where T : BaseEntity
        => IAsyncCursorSourceExtensions.ToList(findFluent);

    public static List<T> ToList<T, TDbContext>(
        this IFindFluent<T, T> findFluent,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default)
        where T : BaseEntity
        where TDbContext : MongoDbContext
        => ToListFromIAsyncCursorSource(findFluent, includeReferences, dbContext);

    private static List<TDocument> ToListFromIAsyncCursorSource<TDocument, TDbContext>(
        this IAsyncCursorSource<TDocument> source,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default,
        CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        using var cursor = source.ToCursor(cancellationToken);
        return cursor.ToListFromIAsyncCursor(includeReferences, dbContext, cancellationToken);
    }

    private static List<TDocument> ToListFromIAsyncCursor<TDocument, TDbContext>(
        this IAsyncCursor<TDocument> cursor,
        List<IncludeReference>? includeReferences = null,
        TDbContext? dbContext = default,
        CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        var list = new List<TDocument>();
        while (cursor.MoveNext(cancellationToken))
        {
            foreach (var item in cursor.Current)
                list.Add(SetReferences(item, includeReferences, dbContext)!);
        }
        return list;
    }

    public static Task<List<T>> ToListAsync<T, TDbContext>(
        this IFindFluent<T, T> findFluent,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default)
        where T : BaseEntity
        where TDbContext : MongoDbContext
        => ToListFromIAsyncCursorSourceAsync(findFluent, includeReferences, dbContext);

    private static Task<List<TDocument>> ToListFromIAsyncCursorSourceAsync<TDocument, TDbContext>(
        this IAsyncCursorSource<TDocument> source,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default,
        CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        using var cursor = source.ToCursor(cancellationToken);
        return cursor.ToListFromIAsyncCursorAsync(includeReferences, dbContext, cancellationToken);
    }

    private static async Task<List<TDocument>> ToListFromIAsyncCursorAsync<TDocument, TDbContext>(
        this IAsyncCursor<TDocument> cursor,
        List<IncludeReference>? includeReferences = null,
        TDbContext? dbContext = default,
        CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        var list = new List<TDocument>();
        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var item in cursor.Current)
                list.Add(SetReferences(item, includeReferences, dbContext)!);
        }
        return list;
    }

    #endregion

    #region FirstOrDefault without expression

    public static TProjection? FirstOrDefault<TDocument, TProjection>(this IFindFluent<TDocument, TProjection> find)
        where TProjection : BaseEntity
        => IAsyncCursorSourceExtensions.FirstOrDefault(find);

    public static Task<TProjection?> FirstOrDefaultAsync<TDocument, TProjection, TDbContext>(
        this IFindFluent<TDocument, TProjection> find,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default,
        CancellationToken cancellationToken = default)
        where TProjection : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        return Task.FromResult(find.Limit(1).FirstOrDefaultWithReferences(includeReferences, dbContext, cancellationToken));
    }

    public static TProjection? FirstOrDefault<TDocument, TProjection, TDbContext>(
        this IFindFluent<TDocument, TProjection> find,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default)
        where TProjection : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        return find.Limit(1).FirstOrDefaultWithReferences(includeReferences, dbContext);
    }

    private static TDocument? FirstOrDefaultWithReferences<TDocument, TDbContext>(
        this IAsyncCursorSource<TDocument> source,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default,
        CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        TDocument? document;
        if (source is IQueryable<TDocument> queryable && !cancellationToken.CanBeCanceled)
            document = Queryable.FirstOrDefault(queryable);
        else
        {
            using var cursor = source.ToCursor(cancellationToken);
            document = cursor.FirstOrDefaultInternal(cancellationToken);
        }
        return SetReferences(document, includeReferences, dbContext);
    }

    private static TDocument? FirstOrDefaultInternal<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default)
    {
        using (cursor)
        {
            var batch = cursor.MoveNext(cancellationToken) ? cursor.Current : Enumerable.Empty<TDocument>();
            return batch.FirstOrDefault();
        }
    }

    #endregion

    #region FirstOrDefault with expression

    public static Task<TDocument?> FirstOrDefaultAsync<TDocument, TDbContext>(
        this IFindFluent<TDocument, TDocument> find,
        Expression<Func<TDocument, bool>> filter,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default,
        CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        find.Filter = filter;
        return Task.FromResult(find.Limit(1).FirstOrDefaultWithReferences(includeReferences, dbContext, cancellationToken));
    }

    public static TDocument? FirstOrDefault<TDocument, TDbContext>(
        this IFindFluent<TDocument, TDocument> find,
        Expression<Func<TDocument, bool>> filter,
        List<IncludeReference>? includeReferences = default,
        TDbContext? dbContext = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        find.Filter = filter;
        return find.Limit(1).FirstOrDefaultWithReferences(includeReferences, dbContext);
    }

    #endregion

    #region Include

    public static IIncludableQueryable<T, TProperty> Include<T, TProperty>(this IFindFluent<T, T> findFluent, Expression<Func<T, TProperty>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var property = ExtractProperty(includeExpression);
        var foreignKeyProperty = FindForeignKey(typeof(TProperty), typeof(T))
            ?? throw new InvalidOperationException($"Foreign key not found on {typeof(TProperty).Name} pointing back to {typeof(T).Name}.");

        var dbContext = StaticServiceLocator.DbContext
            ?? throw new InvalidOperationException("MongoDbContext has not been initialized.");

        var reference = new IncludeReference
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = typeof(T).Name.Pluralize().Underscore()
            },
            Source = new()
            {
                CollectionName = property.PropertyType.Name.Pluralize().Underscore(),
                PropertyInfo = foreignKeyProperty
            }
        };

        return new IncludableQueryable<T, TProperty>(new Collection<T>(dbContext, null), [reference]);
    }

    public static IIncludableQueryable<T, TProperty> IncludeRef<T, TProperty>(this IFindFluent<T, T> findFluent, Expression<Func<T, TProperty>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var property = ExtractProperty(includeExpression);
        var refProperty = typeof(T).GetProperties()
            .FirstOrDefault(p =>
            {
                var attr = p.GetCustomAttribute<ReferenceTo>();
                return attr is not null && !string.IsNullOrEmpty(attr.Entity);
            });

        var dbContext = StaticServiceLocator.DbContext
            ?? throw new InvalidOperationException("MongoDbContext has not been initialized.");

        var reference = new IncludeReference
        {
            EqualityProperty = refProperty!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = typeof(T).Name.Pluralize().Underscore()
            },
            Source = new()
            {
                CollectionName = typeof(TProperty).Name.Pluralize().Underscore(),
                PropertyInfo = typeof(TProperty).GetProperty("Id")
            }
        };

        return new IncludableQueryable<T, TProperty>(new Collection<T>(dbContext, null), [reference]);
    }

    public static IIncludableQueryable<T, TProperty> Include<T, TProperty>(this IFindFluent<T, T> findFluent, Expression<Func<T, IEnumerable<TProperty>>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var property = ExtractProperty(includeExpression);
        var foreignKeyProperty = FindForeignKey(typeof(TProperty), typeof(T))
            ?? throw new InvalidOperationException($"Foreign key not found on {typeof(TProperty).Name} pointing back to {typeof(T).Name}.");

        var dbContext = StaticServiceLocator.DbContext
            ?? throw new InvalidOperationException("MongoDbContext has not been initialized.");

        var reference = new IncludeReference
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = typeof(T).Name.Pluralize().Underscore()
            },
            Source = new()
            {
                CollectionName = typeof(TProperty).Name.Pluralize().Underscore(),
                PropertyInfo = foreignKeyProperty
            }
        };

        return new IncludableQueryable<T, TProperty>(new Collection<T>(dbContext, null), [reference], findFluent.Filter);
    }

    #endregion

    public static string GetCollectionName<TEntity>(this Type entity) where TEntity : BaseEntity
        => typeof(TEntity).Name.Pluralize().Underscore();

    #region Helpers

    internal static PropertyInfo ExtractProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var body = propertyExpression.Body as MemberExpression
            ?? (propertyExpression.Body as UnaryExpression)?.Operand as MemberExpression;
        if (body?.Member is not PropertyInfo propertyInfo)
            throw new ArgumentException("Expression does not refer to a property.", nameof(propertyExpression));
        return propertyInfo;
    }

    internal static PropertyInfo? FindForeignKey(Type entityType, Type ownerType)
    {
        foreach (var fk in entityType.GetProperties())
        {
            var attr = fk.GetCustomAttribute<ForeignKeyTo>();
            if (attr is null) continue;
            if (string.Equals(attr.Entity, ownerType.Name, StringComparison.Ordinal))
                return fk;
        }
        return null;
    }

    private static TDocument? SetReferences<TDocument, TDbContext>(TDocument? item, List<IncludeReference>? includeReferences, TDbContext? dbContext)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        if (item is null || includeReferences is null || dbContext is null) return item;

        foreach (var reference in includeReferences.Where(x => x.Order == 1))
        {
            if (reference.Source?.PropertyInfo is null || reference.Destination?.PropertyInfo is null) continue;

            var collection = dbContext.GetCollection<BsonDocument>(reference.Source.CollectionName!);
            if (collection is null) continue;

            var equalityValue = (string?)reference.EqualityProperty.GetValue(item);
            if (string.IsNullOrEmpty(equalityValue)) continue;

            var equalityPropertyName = reference.Source.PropertyInfo.Name;
            if (equalityPropertyName == "Id") equalityPropertyName = "_id";

            var filter = equalityPropertyName == "_id"
                ? Builders<BsonDocument>.Filter.Eq(equalityPropertyName, ObjectId.Parse(equalityValue))
                : Builders<BsonDocument>.Filter.Eq(equalityPropertyName, equalityValue);

            var sourceValues = collection.Find(filter).ToList();
            if (sourceValues.Count == 0) continue;

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

        var deserializedValue = BsonSerializer.Deserialize(sourceValues[0], reference.Destination.PropertyInfo.PropertyType);
        SetNestedReferences(deserializedValue, includeReferences, dbContext);
        return deserializedValue;
    }

    private static void SetNestedReferences(object deserializedItem, List<IncludeReference> includeReferences, MongoDbContext dbContext)
    {
        foreach (var includeReference in includeReferences.Where(x => x.Order == 2))
        {
            if (includeReference.Source?.PropertyInfo is null || includeReference.Destination?.PropertyInfo is null) continue;

            var collection = dbContext.GetCollection<BsonDocument>(includeReference.Source.CollectionName!);
            if (collection is null) continue;

            var filter = Builders<BsonDocument>.Filter.Eq(includeReference.Source.PropertyInfo.Name, ((BaseEntity)deserializedItem).Id);
            var sourceValue = collection.Find(filter).FirstOrDefault();
            if (sourceValue is null) continue;

            var deserializedValue = BsonSerializer.Deserialize(sourceValue, includeReference.Destination.PropertyInfo.PropertyType);
            includeReference.Destination.PropertyInfo.SetValue(deserializedItem, deserializedValue);
        }
    }

    #endregion
}

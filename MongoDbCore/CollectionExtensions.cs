using MongoDbCore.Helpers;

namespace MongoDbCore;

public static class CollectionExtensions
{
    #region Sorting and Filtering
    public static IFindFluent<T, T> OrderBy<T>(this IFindFluent<T, T> findFluent, Expression<Func<T, object>> expression)
        => findFluent.SortBy(expression);

    public static IFindFluent<T, T> OrderByDescending<T>(this IFindFluent<T, T> findFluent, Expression<Func<T, object>> expression)
        => findFluent.SortByDescending(expression);

    public static Task<List<T>> ToListAsync<T>(this IFindFluent<T, T> findFluent)
        => IAsyncCursorSourceExtensions.ToListAsync(findFluent);

    public static List<T> Take<T>(this IFindFluent<T, T> findFluent, int count)
        => findFluent.Limit(count).ToList();
    #endregion

    #region ToList
    public static List<T> ToList<T>(this IFindFluent<T, T> findFluent)
        where T : BaseEntity
        => IAsyncCursorSourceExtensions.ToList(findFluent);

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
        var list = new List<TDocument>();

        while (cursor.MoveNext(cancellationToken))
        {
            foreach (var item in cursor.Current)
            {
                list.Add(SetReferences(item, includeReferences, dbContext)!);
            }
        }

        return list;
    }
    #endregion

    #region FirstOrDefault without expression
    public static TProjection? FirstOrDefault<TDocument, TProjection>(this IFindFluent<TDocument, TProjection> find, CancellationToken cancellationToken = default)
        where TProjection : BaseEntity
        => IAsyncCursorSourceExtensions.FirstOrDefault(find);

    public static TProjection? FirstOrDefault<TDocument, TProjection, TDbContext>(this IFindFluent<TDocument, TProjection> find, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TProjection : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        return find.Limit(1).FirstOrDefault2(includeReferences, dbContext, cancellationToken);
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
    public static TDocument? FirstOrDefault<TDocument>(this IFindFluent<TDocument, TDocument> find, Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
    {
        find.Filter = filter;
        return IAsyncCursorSourceExtensions.FirstOrDefault(find);
    }

    public static TDocument? FirstOrDefault<TDocument, TDbContext>(this IFindFluent<TDocument, TDocument> find, Expression<Func<TDocument, bool>> filter, List<IncludeReference>? includeReferences = default, TDbContext? dbContext = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));
        find.Filter = filter;
        return find.Limit(1).FirstOrDefault2(includeReferences, dbContext, cancellationToken);
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
            var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => (string)x.NamedArguments[0].TypedValue.Value! == typeof(T).Name);
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

    public static IIncludableQueryable<T, TProperty> Include<T, TProperty>(this IFindFluent<T, T> findFluent, Expression<Func<T, IEnumerable<TProperty>>> includeExpression)
        where T : BaseEntity
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

        var collectionName = typeof(TProperty).Name.Pluralize().Underscore();
        var dbContext = StaticServiceLocator.DbContext;
        var sourceCollectionName = typeof(T).Name.Pluralize().Underscore();

        var reference = new IncludeReference()
        {
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

        foreach (var reference in includeReferences.Where(x => x.Order == 1))
        {
            if (reference.Source?.PropertyInfo == null || reference.Destination?.PropertyInfo == null) continue;

            var collection = dbContext.GetCollection<BsonDocument>(reference.Source.CollectionName!);
            if (collection == null) continue;

            var filter = Builders<BsonDocument>.Filter.Eq(reference.Source.PropertyInfo.Name, item.Id);
            var sourceValues = collection.Find(filter).ToList();  // Cast collection to IMongoCollection<BsonDocument> to use Find

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
        foreach (var includeReference in includeReferences.Where(x => x.Order == 2))
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
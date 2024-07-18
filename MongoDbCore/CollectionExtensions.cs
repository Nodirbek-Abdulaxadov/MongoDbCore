namespace MongoDbCore;

public static class CollectionExtensions
{
    public static IFindFluent<T, T> OrderBy<T>(this IFindFluent<T, T> findFluent,
                                             Expression<Func<T, object>> expression)
        => findFluent.SortBy(expression);

    public static IFindFluent<T, T> OrderByDescending<T>(this IFindFluent<T, T> findFluent,
                                                       Expression<Func<T, object>> expression)
        => findFluent.SortByDescending(expression);

    public static Task<List<T>> ToListAsync<T>(this IFindFluent<T, T> findFluent)
        => IAsyncCursorSourceExtensions.ToListAsync(findFluent);

    public static List<T> Take<T>(this IFindFluent<T, T> findFluent, int count)
    {
        return findFluent.Limit(count).ToList();
    }

    public static PropertyInfo ExtractProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        Expression body = propertyExpression.Body;

        // Handle the case where the body is a UnaryExpression (e.g., for value type properties)
        if (body is UnaryExpression unaryExpression)
        {
            body = unaryExpression.Operand;
        }

        if (body is MemberExpression memberExpression)
        {
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("Expression does not refer to a property.", nameof(propertyExpression));
            }
            return propertyInfo;
        }

        throw new ArgumentException("Expression is not a member access expression.", nameof(propertyExpression));
    }


    #region ToList

    public static List<T> ToList<T, TDbContext>(this IFindFluent<T, T> findFluent, List<IncludeReference>? _includeReferences = default, TDbContext? dbContext = default)
        where T : BaseEntity
        where TDbContext : MongoDbContext
    => ToListFromIAsyncCursorSource(findFluent, _includeReferences, dbContext);

    public static List<TDocument> ToListFromIAsyncCursorSource<TDocument, TDbContext>(this IAsyncCursorSource<TDocument> source, 
                                                                          List<IncludeReference>? _includeReferences = default, 
                                                                          TDbContext? dbContext = default, 
                                                                          CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        using (var cursor = source.ToCursor(cancellationToken))
        {
            return cursor.ToListFromIAsyncCursor(_includeReferences, dbContext, cancellationToken);
        }
    }

    public static List<TDocument> ToListFromIAsyncCursor<TDocument, TDbContext>(this IAsyncCursor<TDocument> source, 
                                                                    List<IncludeReference>? includeReferences = null,
                                                                    TDbContext? dbContext = default,
                                                                    CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
        where TDbContext: MongoDbContext
    {
        Ensure.IsNotNull(source, nameof(source));

        var list = new List<TDocument>();

        using (source)
        {
            while (source.MoveNext(cancellationToken))
            {
                foreach (var item in source.Current)
                {
                    list.Add(SetReferences(item, includeReferences, dbContext)!);
                }
            }
        }

        return list;
    }

    #endregion

    #region FirstOrDefault
    public static TProjection? FirstOrDefault<TDocument, TProjection, TDbContext>(this IFindFluent<TDocument, TProjection> find, 
                                                                     List<IncludeReference>? _includeReferences = default, 
                                                                     TDbContext? dbContext = default, 
                                                                     CancellationToken cancellationToken = default(CancellationToken))
        where TProjection : BaseEntity
        where TDbContext : MongoDbContext
    {
        Ensure.IsNotNull(find, nameof(find));

        return find.Limit(1).FirstOrDefault2(_includeReferences, dbContext, cancellationToken);
    }

    public static TDocument? FirstOrDefault2<TDocument, TDbContext>(this IAsyncCursorSource<TDocument> source, 
                                                       List<IncludeReference>? _includeReferences = default, 
                                                       TDbContext? dbContext = default, 
                                                       CancellationToken cancellationToken = default)
        where TDbContext : MongoDbContext
        where TDocument : BaseEntity
    {
        TDocument? document;

        if (source is IQueryable<TDocument> queryable && !cancellationToken.CanBeCanceled)
        {
            document = Queryable.FirstOrDefault(queryable)!;
        }

        using (var cursor = source.ToCursor(cancellationToken))
        {
            document = cursor.FirstOrDefault(cancellationToken);
        }

        document = SetReferences(document, _includeReferences, dbContext);

        return document;
    }

    public static TDocument FirstOrDefault<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default)
    {
        using (cursor)
        {
            var batch = GetFirstBatch(cursor, cancellationToken);
            return batch.FirstOrDefault()!;
        }
    }

    private static IEnumerable<TDocument> GetFirstBatch<TDocument>(IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken)
    {
        if (cursor.MoveNext(cancellationToken))
        {
            return cursor.Current;
        }
        else
        {
            return Enumerable.Empty<TDocument>();
        }
    }



    #endregion

    private static TDocument? SetReferences<TDocument, TDbContext>(this TDocument item, List<IncludeReference>? includeReferences, TDbContext? dbContext)
        where TDocument : BaseEntity
        where TDbContext : MongoDbContext
    {
        if (item == null || includeReferences == null || dbContext == null) return item;

        foreach (var reference in includeReferences.Where(x => x.Order == 1))
        {
            var source = reference.Source;
            var destination = reference.Destination;

            if (source?.PropertyInfo == null || destination?.PropertyInfo == null) continue;

            var collectionName = source.CollectionName;
            if (string.IsNullOrEmpty(collectionName)) continue;

            var collection = dbContext!.GetCollection<BsonDocument>(collectionName);
            if (collection == null) continue;

            var filter = Builders<BsonDocument>.Filter.Eq(source.PropertyInfo.Name, item.Id);
            var sourceValues = collection.Find(filter).ToList();

            if (sourceValues == null || !sourceValues.Any()) continue;

            dynamic deserializedValue;

            // Check if the destination property is a collection
            if (typeof(IEnumerable).IsAssignableFrom(destination.PropertyInfo.PropertyType))
            {
                var itemTypeOfCollection = destination.PropertyInfo.PropertyType.GetGenericArguments()[0];
                var collectionValues = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemTypeOfCollection))!;

                foreach (var value in sourceValues)
                {
                    dynamic deserializedItem = BsonSerializer.Deserialize(value, itemTypeOfCollection);

                    foreach (var includeReference in includeReferences.Where(x => x.Order == 2))
                    {
                        var source2 = includeReference.Source;
                        var destination2 = includeReference.Destination;

                        if (source2 is null || destination2 is null) continue;

                        var sourceProperty2 = source2.PropertyInfo;
                        var destinationProperty2 = destination2.PropertyInfo;

                        if (sourceProperty2 is null || destinationProperty2 is null) continue;

                        var collectionName2 = source2.CollectionName;

                        if (collectionName2 is null) continue;

                        var collection2 = dbContext?.GetCollection<BsonDocument>(collectionName2);

                        if (collection2 is null) continue;

                        var filter2 = Builders<BsonDocument>.Filter.Eq(sourceProperty2.Name, (string)deserializedItem.Id);

                        var sourceValue2 = collection2.Find(filter2).FirstOrDefault();

                        if (sourceValue2 is null) continue;

                        // Deserialize the dynamic object to the expected type
                        var deserializedValue2 = BsonSerializer.Deserialize(sourceValue2, destinationProperty2.PropertyType);

                        destinationProperty2.SetValue(deserializedItem, deserializedValue2);
                    }
                    collectionValues.Add(deserializedItem);
                }
                deserializedValue = collectionValues;
            }
            else
            {
                deserializedValue = BsonSerializer.Deserialize(sourceValues.FirstOrDefault(), destination.PropertyInfo.PropertyType);

                foreach (var includeReference in includeReferences.Where(x => x.Order == 2))
                {
                    var source2 = includeReference.Source;
                    var destination2 = includeReference.Destination;

                    if (source2 is null || destination2 is null) continue;

                    var sourceProperty2 = source2.PropertyInfo;
                    var destinationProperty2 = destination2.PropertyInfo;

                    if (sourceProperty2 is null || destinationProperty2 is null) continue;

                    var collectionName2 = source2.CollectionName;

                    if (collectionName2 is null) continue;

                    var collection2 = dbContext?.GetCollection<BsonDocument>(collectionName2);

                    if (collection2 is null) continue;

                    var filter2 = Builders<BsonDocument>.Filter.Eq(sourceProperty2.Name, (string)deserializedValue.Id);

                    var sourceValue2 = collection2.Find(filter2).FirstOrDefault();

                    if (sourceValue2 is null) continue;

                    // Deserialize the dynamic object to the expected type
                    var deserializedValue2 = BsonSerializer.Deserialize(sourceValue2, destinationProperty2.PropertyType);

                    destinationProperty2.SetValue(deserializedValue, deserializedValue2);
                }
            }

            destination.PropertyInfo.SetValue(item, deserializedValue);
        }

        return item;
    }
}
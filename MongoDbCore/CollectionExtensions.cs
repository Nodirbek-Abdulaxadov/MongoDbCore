using MongoDB.Driver.Core.Misc;
using MongoDbCore.Helpers;
using System.Reflection;

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

    public static List<T> ToList<T>(this IFindFluent<T, T> findFluent, List<IncludeReference>? _includeReferences = default)
        where T : BaseEntity
    => ToListFromIAsyncCursorSource(findFluent, _includeReferences);

    public static List<TDocument> ToListFromIAsyncCursorSource<TDocument>(this IAsyncCursorSource<TDocument> source, List<IncludeReference>? _includeReferences = default, CancellationToken cancellationToken = default)
        where TDocument : BaseEntity
    {
        using (var cursor = source.ToCursor(cancellationToken))
        {
            return cursor.ToListFromIAsyncCursor(_includeReferences, cancellationToken);
        }
    }

    public static List<TDocument> ToListFromIAsyncCursor<TDocument>(this IAsyncCursor<TDocument> source, List<IncludeReference>? includeReferences = null, CancellationToken cancellationToken = default)
    where TDocument : BaseEntity
    {
        Ensure.IsNotNull(source, nameof(source));

        var list = new List<TDocument>();

        using (source)
        {
            while (source.MoveNext(cancellationToken))
            {
                foreach (var item in source.Current)
                {
                    list.Add(SetReferences(item, includeReferences));
                }
            }
        }

        return list;
    }

    #endregion

    #region FirstOrDefault
    public static TProjection FirstOrDefault<TDocument, TProjection>(this IFindFluent<TDocument, TProjection> find, List<IncludeReference>? _includeReferences = default, CancellationToken cancellationToken = default(CancellationToken))
        where TProjection : BaseEntity
    {
        Ensure.IsNotNull(find, nameof(find));

        return find.Limit(1).FirstOrDefault2(_includeReferences, cancellationToken);
    }

    public static TDocument FirstOrDefault2<TDocument>(this IAsyncCursorSource<TDocument> source, List<IncludeReference>? _includeReferences = default, CancellationToken cancellationToken = default)
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

        document = SetReferences(document, _includeReferences);

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

    private static TDocument? SetReferences<TDocument>(this TDocument item, List<IncludeReference>? includeReferences)
        where TDocument : BaseEntity
    {
        if (item is null) return item;

        // Handle primary includes
        if (includeReferences is not null && includeReferences.Any())
        {
            foreach (var reference in includeReferences.Where(x => x.Id.Equals(item.Id)))
            {
                if (reference != null)
                {
                    reference.Property.SetValue(item, reference.Value);

                    // Handle nested includes (ThenIncludes)
                    var nestedIncludeReferences = includeReferences
                        .Where(x => x.Id.Equals(reference.Value?.GetType().GetProperty("Id")?.GetValue(reference.Value)))
                        .ToList();

                    if (nestedIncludeReferences.Any())
                    {
                        foreach (var nestedReference in nestedIncludeReferences)
                        {
                            nestedReference.Property.SetValue(reference.Value, nestedReference.Value);
                        }
                    }
                }
            }
        }

        return item;
    }
}
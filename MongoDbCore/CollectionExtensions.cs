using MongoDB.Driver.Core.Misc;
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

    public static List<T> Include<T, TProperty>(this IFindFluent<T, T> source, Expression<Func<T, TProperty>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
        => source.ToList().Include(includeExpression);

    public static List<T> Include<T, TProperty>(this List<T> source, Expression<Func<T, TProperty>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var dbContext = StaticServiceLocator.GetService<MongoDbContext>();
        var property = ExtractProperty(includeExpression);

        var propertyProperties = typeof(TProperty).GetProperties();
        var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKey>() is not null).ToList();
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

        for (int i = 0; i < source.Count; i++)
        {
            var filter = Builders<TProperty>.Filter.Eq(foreignKeyPropertyName, source[i].Id);

            var collectionName = property.PropertyType.Name.Pluralize().Underscore();
            var collection = dbContext.GetCollection<TProperty>(collectionName);

            // Create a projection to include the necessary fields
            var projection = Builders<TProperty>.Projection.Include(foreignKeyPropertyName);

            var foreignPropertyValue = collection.Find(filter).Project<TProperty>(projection).FirstOrDefault();

            if (foreignPropertyValue != null)
            {
                lock (property)
                {
                    property.SetValue(source[i], foreignPropertyValue);
                }
            }
        }

        return source;
    }

    public static List<T> Include<T, TProperty>(this IFindFluent<T, T> source, Expression<Func<T, IEnumerable<TProperty>>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
        => source.ToList().Include(includeExpression);

    public static List<T> Include<T, TProperty>(this List<T> source, Expression<Func<T, IEnumerable<TProperty>>> includeExpression)
        where T : BaseEntity
        where TProperty : BaseEntity
    {
        var dbContext = StaticServiceLocator.GetService<MongoDbContext>();
        var property = ExtractProperty(includeExpression);

        var propertyProperties = typeof(TProperty).GetProperties();
        var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKey>() is not null).ToList();
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

        for (int i = 0; i < source.Count; i++)
        {
            var filter = Builders<TProperty>.Filter.Eq(foreignKeyPropertyName, source[i].Id);

            var collectionName = property.PropertyType.Name.Pluralize().Underscore();
            var collection = dbContext.GetCollection<TProperty>(collectionName);

            // Create a projection to include the necessary fields
            var projection = Builders<TProperty>.Projection.Include(foreignKeyPropertyName);

            var foreignPropertyValue = collection.Find(filter).Project<TProperty>(projection).ToList();

            if (foreignPropertyValue != null)
            {
                lock (property)
                {
                    property.SetValue(source[i], foreignPropertyValue);
                }
            }
        }

        return source;
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


    public static List<T> ToList<T>(this IFindFluent<T, T> findFluent, Dictionary<PropertyInfo, dynamic> dict)
        => ToListFromIAsyncCursorSource(findFluent, dict);

    public static List<TDocument> ToListFromIAsyncCursorSource<TDocument>(this IAsyncCursorSource<TDocument> source, Dictionary<PropertyInfo, dynamic> dict, CancellationToken cancellationToken = default)
    {
        using (var cursor = source.ToCursor(cancellationToken))
        {
            return cursor.ToListFromIAsyncCursor(dict, cancellationToken);
        }
    }
    
    public static List<TDocument> ToListFromIAsyncCursor<TDocument>(this IAsyncCursor<TDocument> source, Dictionary<PropertyInfo, dynamic> dict, CancellationToken cancellationToken = default)
    {
        Ensure.IsNotNull(source, nameof(source));
        var list = new List<TDocument>();

        using (source)
        {
            while (source.MoveNext(cancellationToken))
            {
                foreach (var item in source.Current)
                {
                    foreach (var key in dict.Keys)
                    {
                        var value = dict[key];
                        if (value != null)
                        {
                            key.SetValue(item, value);
                        }
                    }
                    list.Add(item);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        return list;
    }
}
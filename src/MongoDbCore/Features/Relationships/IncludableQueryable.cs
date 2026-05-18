namespace MongoDbCore;

/// <summary>
/// Default <see cref="IIncludableQueryable{T,T2}"/> implementation: collects
/// <see cref="IncludeReference"/> entries and forwards execution to the
/// underlying <see cref="Collection{T}"/>.
/// </summary>
public class IncludableQueryable<T, T2>(
    Collection<T> collection,
    List<IncludeReference> includeReferences,
    FilterDefinition<T>? filter = null)
    : IIncludableQueryable<T, T2> where T : BaseEntity
{
    Collection<T> IIncludableQueryable<T, T2>.Collection => collection;
    private List<IncludeReference> IncludeReferences { get; } = includeReferences;
    public List<IncludeReference> GetIncludeReferences() => IncludeReferences;

    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> include)
    {
        var property = CollectionExtensions.ExtractProperty(include);
        var collectionName = property.PropertyType.Name.Pluralize().Underscore();
        PropertyInfo? foreignKeyProperty;

        if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        {
            var itemTypeOfCollection = property.PropertyType.GetGenericArguments()[0];
            collectionName = itemTypeOfCollection.Name.Pluralize().Underscore();
            foreignKeyProperty = CollectionExtensions.FindForeignKey(itemTypeOfCollection, typeof(T))
                ?? throw new InvalidOperationException($"Foreign key not found on {itemTypeOfCollection.Name} pointing back to {typeof(T).Name}.");
        }
        else
        {
            foreignKeyProperty = CollectionExtensions.FindForeignKey(typeof(TProperty), typeof(T));
            if (foreignKeyProperty is null)
                return new IncludableQueryable<T, TProperty>(collection, IncludeReferences);
        }

        IncludeReferences.Add(new IncludeReference
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = collection.Source.CollectionNamespace.CollectionName
            },
            Source = new()
            {
                CollectionName = collectionName,
                PropertyInfo = foreignKeyProperty
            }
        });

        return new IncludableQueryable<T, TProperty>(collection, IncludeReferences);
    }

    public IIncludableQueryable<T, TProperty> IncludeRef<TProperty>(Expression<Func<T, TProperty>> include)
    {
        var property = CollectionExtensions.ExtractProperty(include);
        var refProperty = typeof(T).GetProperties()
            .FirstOrDefault(p =>
            {
                var attr = p.GetCustomAttribute<ReferenceTo>();
                return attr is not null && !string.IsNullOrEmpty(attr.Entity);
            });

        IncludeReferences.Add(new IncludeReference
        {
            EqualityProperty = refProperty!,
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = collection.Source.CollectionNamespace.CollectionName
            },
            Source = new()
            {
                CollectionName = typeof(TProperty).Name.Pluralize().Underscore(),
                PropertyInfo = typeof(TProperty).GetProperty("Id")
            }
        });

        return new IncludableQueryable<T, TProperty>(collection, IncludeReferences);
    }

    public IIncludableQueryable<T, TProperty> ThenInclude<TPreviousProperty, TProperty>(Expression<Func<TPreviousProperty, TProperty>> include)
        => new IncludableQueryable<T, TProperty>(collection, IncludeReferences);

    public IIncludableQueryable<T, TProperty> ThenInclude<TProperty>(Expression<Func<T2, TProperty>> include)
    {
        var property = CollectionExtensions.ExtractProperty(include);
        var collectionName = property.PropertyType.Name.Pluralize().Underscore();
        PropertyInfo? foreignKeyProperty;

        if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        {
            var itemTypeOfCollection = property.PropertyType.GetGenericArguments()[0];
            collectionName = itemTypeOfCollection.Name.Pluralize().Underscore();
            foreignKeyProperty = CollectionExtensions.FindForeignKey(itemTypeOfCollection, typeof(T2))
                ?? throw new InvalidOperationException($"Foreign key not found on {itemTypeOfCollection.Name} pointing back to {typeof(T2).Name}.");
        }
        else
        {
            foreignKeyProperty = CollectionExtensions.FindForeignKey(typeof(TProperty), typeof(T2))
                ?? throw new InvalidOperationException($"Foreign key not found on {typeof(TProperty).Name} pointing back to {typeof(T2).Name}.");
        }

        IncludeReferences.Add(new IncludeReference
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
            Order = 2,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = typeof(T2).Name.Pluralize().Underscore()
            },
            Source = new()
            {
                CollectionName = collectionName,
                PropertyInfo = foreignKeyProperty
            }
        });

        return new IncludableQueryable<T, TProperty>(collection, IncludeReferences);
    }

    public IFindFluent<T, T> Where<TProperty>(Expression<Func<T, bool>> predicate) => collection.Where(predicate);

    public long Count<TProperty>(Expression<Func<T, bool>> predicate) => collection.Count();

    public List<T> ToList()
    {
        filter ??= FilterDefinition<T>.Empty;
        var result = collection.Where(filter).ToList(IncludeReferences, collection.DbContext);
        IncludeReferences.Clear();
        return result;
    }

    public Task<List<T>> ToListAsync()
    {
        filter ??= FilterDefinition<T>.Empty;
        var result = collection.Where(filter).ToListAsync(IncludeReferences, collection.DbContext);
        IncludeReferences.Clear();
        return result;
    }

    public T? FirstOrDefault()
    {
        var item = collection.FirstOrDefault();
        IncludeReferences.Clear();
        return item;
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        var item = collection.FirstOrDefault(predicate);
        IncludeReferences.Clear();
        return item;
    }

    public Task<T?> FirstOrDefaultAsync()
    {
        var item = collection.FirstOrDefaultAsync();
        IncludeReferences.Clear();
        return item;
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var item = collection.FirstOrDefaultAsync(predicate);
        IncludeReferences.Clear();
        return item;
    }
}

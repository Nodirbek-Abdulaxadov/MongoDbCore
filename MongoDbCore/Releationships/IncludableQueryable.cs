namespace MongoDbCore.Releationships;

public class IncludableQueryable<T, T2>(Collection<T> collection, List<IncludeReference> includeReferences) 
    : IIncludableQueryable<T, T2> where T : BaseEntity
{
    #region Fields

    Collection<T> IIncludableQueryable<T, T2>.Collection => collection;
    private List<IncludeReference> IncludeReferences { get; set; } = includeReferences;

    public List<IncludeReference> GetIncludeReferences() => IncludeReferences;

    #endregion

    #region Methods

    /// <inheritdoc />
    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> include)
    {
        var property = CollectionExtensions.ExtractProperty(include);
        PropertyInfo? foreignKeyProperty = null;
        var collectionName = property.PropertyType.Name.Pluralize().Underscore();

        if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        {
            var itemTypeOfCollection = property.PropertyType.GetGenericArguments()[0];
            collectionName = itemTypeOfCollection.Name.Pluralize().Underscore();
            var propertyProperties = itemTypeOfCollection.GetProperties();
            var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKeyTo>() is not null).ToList();
            if (!foreignKeyProperties.Any())
            {
                throw new Exception("Foreign key attribute is not found.");
            }

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
        }
        else
        {
            var propertyProperties = typeof(TProperty).GetProperties();
            var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKeyTo>() is not null).ToList();
            if (!foreignKeyProperties.Any())
            {
                throw new Exception("Foreign key attribute is not found.");
            }

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
        }

        var reference = new IncludeReference()
        {
            Order = 1,
            Destination = new()
            {
                PropertyInfo = property,
                CollectionName = collection.Source!.CollectionNamespace.CollectionName
            },
            Source = new()
            {
                CollectionName = collectionName,
                PropertyInfo = foreignKeyProperty
            }
        };
        IncludeReferences.Add(reference);

        return new IncludableQueryable<T, TProperty>(collection, IncludeReferences);
    }

    /// <inheritdoc />
    public IIncludableQueryable<T, TProperty> ThenInclude<TPreviousProperty, TProperty>(Expression<Func<TPreviousProperty, TProperty>> include)
    {
        return new IncludableQueryable<T, TProperty>(collection, IncludeReferences);
    }

    /// <inheritdoc />
    public IIncludableQueryable<T, TProperty> ThenInclude<TProperty>(Expression<Func<T2, TProperty>> include)
    {
        var property = CollectionExtensions.ExtractProperty(include);
        PropertyInfo? foreignKeyProperty = null;
        var collectionName = property.PropertyType.Name.Pluralize().Underscore();

        if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        {
            var itemTypeOfCollection = property.PropertyType.GetGenericArguments()[0];
            collectionName = itemTypeOfCollection.Name.Pluralize().Underscore();
            var propertyProperties = itemTypeOfCollection.GetProperties();
            var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKeyTo>() is not null).ToList();
            if (!foreignKeyProperties.Any())
            {
                throw new Exception("Foreign key attribute is not found.");
            }

            foreach (var fkProperty in foreignKeyProperties)
            {
                var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => (string)x.NamedArguments[0].TypedValue.Value! == typeof(T2).Name);
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
        }
        else
        {
            var propertyProperties = typeof(TProperty).GetProperties();
            var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKeyTo>() is not null).ToList();
            if (!foreignKeyProperties.Any())
            {
                throw new Exception("Foreign key attribute is not found.");
            }

            foreach (var fkProperty in foreignKeyProperties)
            {
                var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => (string)x.NamedArguments[0].TypedValue.Value! == typeof(T2).Name);
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
        }

        var reference = new IncludeReference()
        {
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
        };
        IncludeReferences.Add(reference);

        return new IncludableQueryable<T, TProperty>(collection, IncludeReferences);
    }

    public IFindFluent<T, T> Where<TProperty>(Expression<Func<T, bool>> predicate)
        => collection.Where(predicate);

    public long Count<TProperty>(Expression<Func<T, bool>> predicate)
        => collection.Count();

    public List<T> ToList()
    {
        var result = CollectionExtensions.ToList(collection.AsFindFluent(), IncludeReferences, collection.DbContext);
        IncludeReferences.Clear();
        return result;
    }

    public T? FirstOrDefault()
    {
        IncludeReferences.Clear();
        return collection.FirstOrDefault();
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        IncludeReferences.Clear();
        return collection.FirstOrDefault(predicate);
    }

    #endregion
}
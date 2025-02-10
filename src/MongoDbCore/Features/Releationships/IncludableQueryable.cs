public class IncludableQueryable<T, T2>(Collection<T> collection, List<IncludeReference> includeReferences, FilterDefinition<T>? filter = null)
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
                var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => x.NamedArguments is not null &&
                                                                                x.NamedArguments.Any() &&
                                                                                (string)x.NamedArguments[0].TypedValue.Value! == typeof(TProperty).Name ||
                                                                                x.ConstructorArguments is not null &&
                                                                                x.ConstructorArguments.Any() &&
                                                                                (string)x.ConstructorArguments[0].Value! == typeof(TProperty).Name);
                if (attribute is null)
                {
                    continue;
                }

                foreignKeyProperty = fkProperty;
                break;
            }

            if (foreignKeyProperty == null)
            {
                return new IncludableQueryable<T, TProperty>(collection, IncludeReferences); ;
            }
        }

        var reference = new IncludeReference()
        {
            EqualityProperty = typeof(T).GetProperty("Id")!,
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
    public IIncludableQueryable<T, TProperty> IncludeRef<TProperty>(Expression<Func<T, TProperty>> include)
    {
        var property = CollectionExtensions.ExtractProperty(include);

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
                break;
            }
        }

        var collectionName = typeof(TProperty).Name.Pluralize().Underscore();

        IncludeReferences.Add(
            new IncludeReference()
            {
                EqualityProperty = refProperty!,
                Order = 1,
                Destination = new()
                {
                    PropertyInfo = property,
                    CollectionName = collection.Source!.CollectionNamespace.CollectionName
                },
                Source = new()
                {
                    CollectionName = collectionName,
                    PropertyInfo = typeof(TProperty).GetProperty("Id")
                }
            });

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
        var s = typeof(T).Name;
        var s2 = typeof(T2).Name;
        var s3 = typeof(TProperty).Name;

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
                var attribute = fkProperty.CustomAttributes.FirstOrDefault(x => x.NamedArguments is not null &&
                                                                                x.NamedArguments.Any() &&
                                                                                (string)x.NamedArguments[0].TypedValue.Value! == typeof(T2).Name ||
                                                                                x.ConstructorArguments is not null &&
                                                                                x.ConstructorArguments.Any() &&
                                                                                (string)x.ConstructorArguments[0].Value! == typeof(T2).Name);
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
        var items = collection.FirstOrDefault(predicate);
        IncludeReferences.Clear();
        return items;
    }

    public Task<T?> FirstOrDefaultAsync()
    {
        var item = collection.FirstOrDefaultAsync();
        IncludeReferences.Clear();
        return item!;
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var items = collection.FirstOrDefaultAsync(predicate);
        IncludeReferences.Clear();
        return items!;
    }

    #endregion
}
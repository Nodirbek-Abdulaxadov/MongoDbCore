namespace MongoDbCore.Releationships;

public class IncludableQueryable<T, T2>(Collection<T> collection, List<IncludeReference> includeReferences) : IIncludableQueryable<T, T2> where T : BaseEntity
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

        var propertyProperties = typeof(TProperty).GetProperties();
        var foreignKeyProperties = propertyProperties.Where(x => x.GetCustomAttribute<ForeignKeyTo>() is not null).ToList();
        if (!foreignKeyProperties.Any())
        {
            throw new Exception("Foreign key attribute is not found.");
        }

        PropertyInfo? foreignKeyProperty = null;

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

        var collectionName = property.PropertyType.Name.Pluralize().Underscore();


        var reference = new IncludeReference()
        {
            Order = 2,
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

    #endregion
}
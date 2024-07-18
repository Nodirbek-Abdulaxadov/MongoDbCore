namespace MongoDbCore.Releationships;

public interface IIncludableQueryable<T, T2> where T : BaseEntity
{
    #region Properties
    Collection<T> Collection { get; }
    List<IncludeReference> GetIncludeReferences();

    #endregion

    #region Methods
    IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> include);

    IIncludableQueryable<T, TProperty> ThenInclude<TPreviousProperty, TProperty>(Expression<Func<TPreviousProperty, TProperty>> include);

    IIncludableQueryable<T, TProperty> ThenInclude<TProperty>(Expression<Func<T2, TProperty>> include);

    List<T> ToList();

    T? FirstOrDefault();

    T? FirstOrDefault(Expression<Func<T, bool>> predicate);

    #endregion
}
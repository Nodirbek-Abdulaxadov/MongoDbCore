using System;

namespace MongoDbCore;

public static class IncludeExpressions
{
    public static List<T> ToList<T, T2>(this IIncludableQueryable<T, T2> source)
        where T : BaseEntity
    {
        return CollectionExtensions.ToList(source.Collection.AsFindFluent(), source.GetIncludeReferences(), source.Collection.DbContext);
    }

    public static T? FirstOrDefault<T, T2>(this IIncludableQueryable<T, T2> source)
        where T : BaseEntity
    {
        return source.Collection.FirstOrDefault();
    }

    public static T? FirstOrDefault<T, T2>(this IIncludableQueryable<T, T2> source, Expression<Func<T, bool>> predicate)
        where T : BaseEntity
    {
        return source.Collection.FirstOrDefault(predicate);
    }
}
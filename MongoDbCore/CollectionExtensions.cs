namespace MongoDbCore;

public static class CollectionExtensions
{
    public static IFindFluent<T, T> OrderBy<T>(this IFindFluent<T, T> findFluent,
                                             Expression<Func<T, object>> expression)
        => findFluent.SortBy(expression);

    public static IFindFluent<T, T> OrderByDescending<T>(this IFindFluent<T, T> findFluent,
                                                       Expression<Func<T, object>> expression)
        => findFluent.SortByDescending(expression);

    public static List<T> ToList<T>(this IFindFluent<T, T> findFluent)
        => IAsyncCursorSourceExtensions.ToList(findFluent);

    public static Task<List<T>> ToListAsync<T>(this IFindFluent<T, T> findFluent)
        => IAsyncCursorSourceExtensions.ToListAsync(findFluent);

    public static List<T> Take<T>(this IFindFluent<T, T> findFluent, int count)
    {
        return findFluent.Limit(count).ToList();
    }
}
namespace MongoDbCore;

/// <summary>
/// Internal service locator used by a small number of static extension
/// methods (<c>CollectionExtensions.Include</c>, audit service) that have no
/// other way to reach the registered <see cref="MongoDbContext"/>.
/// </summary>
internal static class StaticServiceLocator
{
    internal static IServiceProvider? ServiceProvider { get; set; }
    internal static MongoDbContext? DbContext { get; set; }

    internal static T GetService<T>() where T : notnull
        => ServiceProvider is null
            ? throw new InvalidOperationException("Service provider has not been initialized.")
            : ServiceProvider.GetRequiredService<T>();
}

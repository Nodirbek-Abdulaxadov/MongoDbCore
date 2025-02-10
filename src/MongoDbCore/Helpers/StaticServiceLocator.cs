internal static class StaticServiceLocator
{
    internal static IServiceProvider? ServiceProvider { get; set; }
    internal static MongoDbContext? DbContext { get; set; }

    internal static T GetService<T>()
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}
namespace MongoDbCore.Helpers;

internal static class StaticServiceLocator
{
    internal static IServiceProvider? ServiceProvider { get; set; }

    internal static T GetService<T>()
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}
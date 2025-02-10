public static class MongoDbCoreExtensions
{
    public static void AddMongoDbContext<TDbContext>(this IServiceCollection services, MongoDbCoreOptions? options = null)
        where TDbContext : MongoDbContext // Remove the nullable indicator (?) here
    {
        services.AddSingleton(provider =>
        {
            StaticServiceLocator.ServiceProvider = provider;

            // Create an instance of TDbContext using the provided options
            TDbContext? dbContext;
            if (options == null)
            {
                options = new();
                dbContext = Activator.CreateInstance(typeof(TDbContext)) as TDbContext;
            }
            else
            {
                dbContext = Activator.CreateInstance(typeof(TDbContext), options) as TDbContext;
            }

            var client = new MongoClient(options.Connection);
            var database = client.GetDatabase(options.Database);

            dbContext!.HealthCheckDB();

            // Get all properties of TDbContext
            var properties = typeof(TDbContext).GetProperties();
            foreach (var property in properties)
            {
                // Check if the property type is a generic collection
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Collection<>))
                {
                    // Get the generic type argument of the collection
                    var entityType = property.PropertyType.GetGenericArguments()[0];

                    // Create an instance of Collection<TEntity> with the entity type and database instance
                    var collectionInstance = Activator.CreateInstance(typeof(Collection<>).MakeGenericType(entityType), dbContext);

                    // Set the collection instance to the property
                    property.SetValue(dbContext, collectionInstance);
                }

                // Check if the property type is a generic collection
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(SelfCachedCollection<>))
                {
                    // Get the generic type argument of the collection
                    var entityType = property.PropertyType.GetGenericArguments()[0];

                    // Create an instance of Collection<TEntity> with the entity type and database instance
                    var collectionInstance = Activator.CreateInstance(typeof(SelfCachedCollection<>).MakeGenericType(entityType), dbContext);

                    // Set the collection instance to the property
                    property.SetValue(dbContext, collectionInstance);
                }
            }
            dbContext!.Initialize();
            StaticServiceLocator.DbContext = dbContext;
            return dbContext;
        });
    }
}
namespace EntityFrameworkCore.MongoDb;

public static class MongoDbContextOptionsExtensions
{
    public static void AddMongoDbContext<TDbContext>(this IServiceCollection services, MongoDbCoreOptions options)
        where TDbContext : MongoDbContext // Remove the nullable indicator (?) here
    {
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning disable CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
        services.AddSingleton(provider =>
        {
            var client = new MongoClient(options.Connection);
            var database = client.GetDatabase(options.Database);

            // Create an instance of TDbContext using the provided options
            var dbContext = Activator.CreateInstance(typeof(TDbContext), options) as TDbContext;
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
            }
            dbContext!.Initialize();
            return dbContext;
        });
#pragma warning restore CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.

    }
}
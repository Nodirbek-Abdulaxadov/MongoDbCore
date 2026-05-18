namespace MongoDbCore;

/// <summary>
/// DI registration entry point for <see cref="MongoDbContext"/> derivatives.
/// </summary>
public static class MongoDbCoreExtensions
{
    /// <summary>
    /// Registers a <typeparamref name="TDbContext"/> as a singleton along with
    /// the optional <see cref="IAuditService"/> when auditing is enabled.
    /// </summary>
    public static IServiceCollection AddMongoDbContext<TDbContext>(
        this IServiceCollection services,
        MongoDbCoreOptions? options = null)
        where TDbContext : MongoDbContext
    {
        options ??= new MongoDbCoreOptions();

        if (options.EnableAuditing)
            services.AddSingleton<IAuditService, AuditService>();

        services.AddSingleton(provider =>
        {
            StaticServiceLocator.ServiceProvider = provider;

            TDbContext dbContext = (Activator.CreateInstance(typeof(TDbContext), options) as TDbContext)
                ?? throw new InvalidOperationException(
                    $"Type '{typeof(TDbContext).FullName}' is missing a public constructor accepting MongoDbCoreOptions.");

            dbContext.HealthCheckDb();

            IAuditService? auditService = options.EnableAuditing
                ? provider.GetRequiredService<IAuditService>()
                : null;

            PopulateCollectionProperties(dbContext, auditService);

            if (options.EnableAuditing)
            {
                dbContext.Audits = new Collection<AuditEntity>(dbContext, null);
            }

            dbContext.Initialize();
            StaticServiceLocator.DbContext = dbContext;
            return dbContext;
        });

        return services;
    }

    private static void PopulateCollectionProperties<TDbContext>(TDbContext dbContext, IAuditService? auditService)
        where TDbContext : MongoDbContext
    {
        foreach (var property in typeof(TDbContext).GetProperties())
        {
            if (!property.PropertyType.IsGenericType) continue;

            var generic = property.PropertyType.GetGenericTypeDefinition();
            var entityType = property.PropertyType.GetGenericArguments()[0];

            if (generic == typeof(Collection<>))
            {
                var instance = Activator.CreateInstance(
                    typeof(Collection<>).MakeGenericType(entityType),
                    dbContext,
                    auditService);
                property.SetValue(dbContext, instance);
            }
            else if (generic == typeof(SelfCachedCollection<>))
            {
                var instance = Activator.CreateInstance(
                    typeof(SelfCachedCollection<>).MakeGenericType(entityType),
                    dbContext);
                property.SetValue(dbContext, instance);
            }
        }
    }
}

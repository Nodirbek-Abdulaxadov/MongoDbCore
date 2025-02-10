namespace MongoDbCore.Identity.External;

public static class IdentityBuilderExtensions
{
    /// <summary>
    /// Registers the MongoDb identity services with custom user and role types.
    /// </summary>
    /// <typeparam name="TDbContext">The custom identity DbContext type derived from IdentityDbContext.</typeparam>
    /// <typeparam name="TUser">The custom user type derived from IdentityUser.</typeparam>
    /// <typeparam name="TRole">The custom role type derived from IdentityRole.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="options">The options to configure MongoDb.</param>
    /// <returns>The updated IServiceCollection with MongoDb identity services.</returns>
    public static IServiceCollection AddMongoDbIdentity<TDbContext, TUser, TRole>(
        this IServiceCollection services, MongoDbCoreOptions? options)
            where TDbContext : IdentityDbContext<TUser, TRole>
            where TUser : IdentityUser
            where TRole : IdentityRole
    {
        // Register the custom DbContext as a singleton, allowing it to be injected throughout the application.
        services.AddMongoDbContext<TDbContext>(options);
        
        // Register the UserManager service with the custom user type (TUser).
        services.AddScoped<IUserManager<TUser>, UserManager<TUser>>();
        
        // Register the RoleManager service with the custom role type (TRole).
        services.AddScoped<IRoleManager<TRole>, RoleManager<TRole>>();

        return services;
    }

    /// <summary>
    /// Registers the MongoDb identity services with a custom user type and the default role type (IdentityRole).
    /// </summary>
    /// <typeparam name="TDbContext">The custom identity DbContext type derived from IdentityDbContext.</typeparam>
    /// <typeparam name="TUser">The custom user type derived from IdentityUser.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="options">The options to configure MongoDb.</param>
    /// <returns>The updated IServiceCollection with MongoDb identity services.</returns>
    public static IServiceCollection AddMongoDbIdentity<TDbContext, TUser>(
        this IServiceCollection services, MongoDbCoreOptions? options)
            where TDbContext : IdentityDbContext<TUser>
            where TUser : IdentityUser
    {
        // Register the custom DbContext as a singleton.
        services.AddMongoDbContext<TDbContext>(options);
        
        // Register the UserManager service with the custom user type (TUser).
        services.AddScoped<IUserManager<TUser>, UserManager<TUser>>();
        
        // Register the RoleManager service with the default IdentityRole type.
        services.AddScoped<IRoleManager<IdentityRole>, RoleManager<IdentityRole>>();

        return services;
    }

    /// <summary>
    /// Registers the MongoDb identity services with the default user (IdentityUser) and role (IdentityRole) types.
    /// </summary>
    /// <typeparam name="TDbContext">The identity DbContext type derived from IdentityDbContext.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="options">The options to configure MongoDb.</param>
    /// <returns>The updated IServiceCollection with MongoDb identity services.</returns>
    public static IServiceCollection AddMongoDbIdentity<TDbContext>(
        this IServiceCollection services, MongoDbCoreOptions? options)
            where TDbContext : IdentityDbContext<IdentityUser, IdentityRole>
    {
        // Register the default IdentityDbContext as a singleton.
        services.AddMongoDbContext<TDbContext>(options);
        
        // Register the UserManager service with the default IdentityUser type.
        services.AddScoped<IUserManager<IdentityUser>, UserManager<IdentityUser>>();
        
        // Register the RoleManager service with the default IdentityRole type.
        services.AddScoped<IRoleManager<IdentityRole>, RoleManager<IdentityRole>>();

        return services;
    }
}

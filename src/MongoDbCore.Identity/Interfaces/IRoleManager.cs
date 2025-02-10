namespace MongoDbCore.Identity.Interfaces;

/// <summary>
/// Represents a manager for roles, providing CRUD operations for <typeparamref name="TRole"/>.
/// </summary>
/// <typeparam name="TRole">The type of the role, which must inherit from <see cref="IdentityRole"/>.</typeparam>
public interface IRoleManager<TRole> : IDisposable where TRole : IdentityRole
{
    Task<List<TRole>> GetRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a role by its identifier.
    /// </summary>
    /// <param name="roleId">The identifier of the role.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the role if found, otherwise null.</returns>
    Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a role by its normalized name.
    /// </summary>
    /// <param name="normalizedRoleName">The normalized name of the role (usually uppercase for case-insensitive comparison).</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the role if found, otherwise null.</returns>
    Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="role">The role to create.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateAsync(TRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    /// <param name="role">The role to update.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(TRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role.
    /// </summary>
    /// <param name="role">The role to delete.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(TRole role, CancellationToken cancellationToken = default);
}

/// <summary>
/// A specialized version of <see cref="IRoleManager{TRole}"/> for the <see cref="IdentityRole"/> type.
/// </summary>
public interface IRoleManager : IRoleManager<IdentityRole>
{
}
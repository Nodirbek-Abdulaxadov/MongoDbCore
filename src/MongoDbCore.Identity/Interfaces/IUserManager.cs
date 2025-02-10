namespace MongoDbCore.Identity.Interfaces;

/// <summary>
/// Represents a manager for user operations, providing CRUD and authentication-related operations for <typeparamref name="TUser"/>.
/// </summary>
/// <typeparam name="TUser">The type of the user, which must inherit from <see cref="IdentityUser"/>.</typeparam>
public interface IUserManager<TUser> : IDisposable where TUser : IdentityUser
{
    /// <summary>
    /// Finds a user by its identifier.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the user if found, otherwise null.</returns>
    Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user by their username.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the user if found, otherwise null.</returns>
    Task<TUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the created user.</returns>
    Task<TUser> CreateAsync(TUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated user.</returns>
    Task<TUser> UpdateAsync(TUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the provided password is correct for the given user.
    /// </summary>
    /// <param name="user">The user to check the password for.</param>
    /// <param name="password">The password to check.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the password is correct, otherwise false.</returns>
    Task<bool> CheckPasswordAsync(TUser user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password for the given user.
    /// </summary>
    /// <param name="user">The user whose password will be changed.</param>
    /// <param name="password">The new password to set.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the password was successfully changed, otherwise false.</returns>
    Task<bool> ChangePasswordAsync(TUser user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(TUser user, CancellationToken cancellationToken);
}

/// <summary>
/// A specialized version of <see cref="IUserManager{TUser}"/> for the <see cref="IdentityUser"/> type.
/// </summary>
public interface IUserManager : IUserManager<IdentityUser>
{
}
namespace MongoDbCore.Identity.Internal;

/// <summary>
/// UserManager provides functionality to manage users in the identity system.
/// Implements the IUserManager interface for handling CRUD operations for users.
/// </summary>
/// <typeparam name="TUser">The type of user. Should be derived from IdentityUser.</typeparam>
public class UserManager<TUser> : IUserManager<TUser> where TUser : IdentityUser
{
    private readonly IdentityDbContext<TUser, IdentityRole> _dbContext;

    /// <summary>
    /// Initializes a new instance of the UserManager class.
    /// </summary>
    /// <param name="dbContext">The IdentityDbContext instance for interacting with the database.</param>
    public UserManager(IdentityDbContext<TUser, IdentityRole> dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "DbContext cannot be null");
    }

    /// <summary>
    /// Changes the password of the user.
    /// </summary>
    /// <param name="user">The user whose password will be changed.</param>
    /// <param name="password">The new password.</param>
    /// <returns>True if the password was successfully changed, otherwise throws an exception.</returns>
    public async Task<bool> ChangePasswordAsync(TUser user, string password, CancellationToken cancellationToken = default)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id && x.UserName == user.UserName);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User not found with Id: {user.Id} and UserName: {user.UserName}.");
        }

        user.PasswordHash = password.HashPassword(); // Hash the new password before saving
        await _dbContext.Users.UpdateAsync(user); // Update the user with the new password hash

        return true;
    }

    /// <summary>
    /// Checks if the provided password is correct for the given user.
    /// </summary>
    /// <param name="user">The user to check the password for.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>True if the password is correct, otherwise false.</returns>
    public async Task<bool> CheckPasswordAsync(TUser user, string password, CancellationToken cancellationToken = default)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id && x.UserName == user.UserName);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User not found with Id: {user.Id} and UserName: {user.UserName}.");
        }

        return existingUser.PasswordHash.VerifyPassword(password); // Verify if the password matches the stored hash
    }

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The created user.</returns>
    public async Task<TUser> CreateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Users.Any(x => x.UserName == user.UserName))
        {
            throw new InvalidOperationException($"Username already exists: {user.UserName}.");
        }

        return await _dbContext.Users.UpdateAsync(user); // Save the user in the database
    }

    /// <summary>
    /// Deletes an existing user from the system.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(TUser user, CancellationToken cancellationToken = default)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id && x.UserName == user.UserName);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User not found with Id: {user.Id} and UserName: {user.UserName}.");
        }

        await _dbContext.Users.DeleteAsync(user, cancellationToken); // Delete the user from the database
    }

    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The user if found, otherwise null.</returns>
    public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Finds a user by their username.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The user if found, otherwise null.</returns>
    public async Task<TUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
    }

    /// <summary>
    /// Updates the details of an existing user.
    /// </summary>
    /// <param name="user">The user with updated details.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The updated user.</returns>
    public async Task<TUser> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id && x.UserName == user.UserName, cancellationToken);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User not found with Id: {user.Id} and UserName: {user.UserName}.");
        }

        return await _dbContext.Users.UpdateAsync(user); // Update user in the database
    }

    /// <summary>
    /// Disposes of the UserManager resources.
    /// </summary>
    public void Dispose()
    {
        // Dispose of any resources if needed
        GC.SuppressFinalize(this);
    }
}

internal class UserManager(IdentityDbContext<IdentityUser, IdentityRole> dbContext) 
    : UserManager<IdentityUser>(dbContext), IUserManager
{

}
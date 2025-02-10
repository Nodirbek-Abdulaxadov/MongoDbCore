namespace MongoDbCore.Identity.Internal;

/// <summary>
/// Provides helper methods for hashing and verifying passwords using BCrypt.
/// </summary>
internal static class PasswordHasher
{
    /// <summary>
    /// Hashes the given password using BCrypt.
    /// </summary>
    /// <param name="password">The plain-text password to be hashed.</param>
    /// <returns>A hashed version of the password.</returns>
    internal static string HashPassword(this string password)
    {
        // Hash the password using BCrypt
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies if the given password matches the hashed password.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hashedPassword">The stored hashed password to compare against.</param>
    /// <returns>True if the password matches the hashed password, otherwise false.</returns>
    internal static bool VerifyPassword(this string password, string hashedPassword)
    {
        // Verify the password against the hashed password using BCrypt
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
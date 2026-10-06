namespace FCS.Utilities.Interfaces;

/// <summary>
/// Defines an interface for encrypting and validating passwords, as well as checking password strength.
/// </summary>
public interface IEncryptor
{
    /// <summary>
    /// Gets the salt value.
    /// </summary>
    /// <returns>The salt value.</returns>
    string GetSalt();

    /// <summary>
    /// Computes a hash of the specified value using the provided salt.
    /// </summary>
    /// <param name="value">The value to hash.</param>
    /// <param name="salt">The salt to use for hashing.</param>
    /// <returns>The computed hash.</returns>
    string GetHash(string value, string salt);

    /// <summary>
    /// Creates a password hash using the specified hash algorithm.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="hashAlgorithm">The hash algorithm to use.</param>
    /// <returns>The hashed password.</returns>
    string CreatePasswordHash(string password, string hashAlgorithm);

    /// <summary>
    /// Validates a password against a hash using a salt.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="hash">The hash to validate against.</param>
    /// <param name="salt">The salt used for hashing.</param>
    /// <returns>true if the password is valid; otherwise, false.</returns>
    bool IsPasswordValid(string password, string hash, string salt);

    /// <summary>
    /// Validates whether a password meets minimum strength requirements.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="minimumLength">The minimum required length for the password.</param>
    /// <returns><see langword="true"/> if the password meets the strength requirements; otherwise, <see langword="false"/>.</returns>
    bool PasswordStrengthCheck(string password, int minimumLength);
}
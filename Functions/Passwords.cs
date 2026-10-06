using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FCS.Utilities.Interfaces;

namespace FCS.Utilities;

/// <summary>
/// Used to Encrypt Passwords
/// </summary>
public partial class Encryptor : IEncryptor
{
    private const int SALT_SIZE = 40;
    private const int ITERATIONS_COUNT = 10000;
    private const int MINIMUM_PASSWORD_LENGTH = 8;

    [GeneratedRegex(@"(?!^[ ]*$)(?!^[0-9]*$)(?!^[a-zA-Z]*$)^(.{8,})$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PasswordStrengthPattern();

    /// <summary>
    /// Generate A Salt Value
    /// </summary>
    /// <returns>Base64 encoded salt string</returns>
    public string GetSalt()
    {
        byte[] saltBytes = new byte[SALT_SIZE];
        RandomNumberGenerator.Fill(saltBytes);

        return Convert.ToBase64String(saltBytes);
    }

    /// <summary>
    /// Generate the Hash based on the supplied Password and Salt
    /// </summary>
    /// <param name="password">Password</param>
    /// <param name="salt">Salt</param>
    /// <returns>Base64 encoded hash</returns>
    public string GetHash(string password, string salt)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(salt);

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltBytes = GetBytes(salt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, ITERATIONS_COUNT, HashAlgorithmName.SHA256, SALT_SIZE);

        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Returns the hash of the given string.
    /// </summary>
    /// <param name="stringToHash">string for which the hash should be generated</param>
    /// <param name="hashAlgorithm">Hash algorithm. Ex: MD5, SHA1, SHA256, SHA384, SHA512</param>
    /// <returns>Hexadecimal hash string</returns>
    public string CreatePasswordHash(string stringToHash, string hashAlgorithm = "SHA256")
    {
        ArgumentNullException.ThrowIfNull(stringToHash);

        // Create Hash using the appropriate algorithm
        using HashAlgorithm algorithm = hashAlgorithm.ToUpperInvariant() switch
        {
            "MD5" => MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA256" => SHA256.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            _ => SHA256.Create() // Default to SHA256
        };

        byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));

        // ToString("x2") converts byte in hexadecimal value
        return string.Concat(hash.Select(b => b.ToString("x2"))).ToUpperInvariant();
    }

    /// <summary>
    /// Validate the password
    /// </summary>
    /// <param name="password">password string</param>
    /// <param name="hash">Stored password hash string</param>
    /// <param name="salt">Stored Salt string</param>
    /// <returns>True/False</returns>
    public bool IsPasswordValid(string password, string hash, string salt) => string.Equals(hash, CreatePasswordHash($"{password}{salt}"), StringComparison.Ordinal);

    /// <summary>
    /// Check the strength of the supplied password.
    /// Password Requirements: 1 number and 1 letter
    /// </summary>
    /// <param name="password">Password to be validated for strength</param>
    /// <param name="length">Minimum acceptable password length (minimum 2)</param>
    /// <returns>True if password meets requirements, false otherwise</returns>
    public bool PasswordStrengthCheck(string password, int length = MINIMUM_PASSWORD_LENGTH)
    {
        if (string.IsNullOrEmpty(password)) return false;
        if (length < 2) length = MINIMUM_PASSWORD_LENGTH;

        return PasswordStrengthPattern().IsMatch(password);
    }

    #region Private Methods

    /// <summary>
    /// Convert String To Byte Array
    /// </summary>
    /// <param name="value">String To Be Decoded</param>
    /// <returns>Byte Array</returns>
    private static byte[] GetBytes(string value)
    {
        byte[] bytes = new byte[value.Length * sizeof(char)];
        Buffer.BlockCopy(value.ToCharArray(), 0, bytes, 0, bytes.Length);

        return bytes;
    }

    #endregion Private Methods
}
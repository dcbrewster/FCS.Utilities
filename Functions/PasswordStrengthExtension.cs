namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Check the strength of the supplied password - extension method version.
    /// Password Requirements: 1 number and 1 letter, minimum 8 characters
    /// </summary>
    /// <param name="password">Password to be validated for strength</param>
    /// <param name="length">Minimum acceptable password length (minimum 2, default 8)</param>
    /// <returns>True if password meets requirements, false otherwise</returns>
    public static bool PasswordStrengthCheck(this string? password, int length = 8)
    {
        Encryptor encryptor = new();

        return encryptor.PasswordStrengthCheck(password ?? string.Empty, length);
    }
}
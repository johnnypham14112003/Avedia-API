using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic.Extensions.Utils;

public static class StringUtils
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 600000; // Suggestion of OWASP in 2024 for PBKDF2-HMAC-SHA512
    public static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    /// <summary>
    ///     This method is use to hash a <paramref name="string"/> using PBKDF2 + Hmac-SHA512,
    ///     then merge it with other params to make it more hard to predict and reusable for the comparator.<para/>
    ///     The comparator method to check/validate them: <see cref="BoolUtils.VerifyPassword">VerifyPassword(password, hashedPassowrd)</see>
    /// </summary>
    /// <param name="password">non-hashed string</param>
    /// <returns>a 3 parts string seperate by "." (dot)<para/> (eg: <b>"1000.123.c3j3h4jh"</b>)</returns>
    public static string HashPassword(string password)
    {
        // Create random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash password using PBKDF2 + Hmac-SHA512
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize
        );

        // Custom return as a string with 3 parts
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    ///     Extract username from <paramref name="email"/><para/>
    ///     Example: <c>"user123@email.com"</c>
    /// </summary>
    /// <returns><c>"user123"</c> or empty string "" if not found <b>@</b></returns>
    public static string GetUsername(string email)
    {
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        // Find index of @"
        ReadOnlySpan<char> emailSpan = email.AsSpan();
        int atIndex = emailSpan.IndexOf('@');

        // If not found @ or @ at first index => null
        if (atIndex <= 0)
            return string.Empty;

        return emailSpan[..atIndex].ToString();
    }

    /// <summary>
    ///     Generate random OTP code numbers.
    /// </summary>
    /// <param name="length">length of the return code</param>
    /// <returns>a string (eg: <b>"783930"</b>)</returns>
    public static string GenerateRandomOTP(int length = 6)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        return RandomNumberGenerator.GetString("0123456789", length);
    }
}

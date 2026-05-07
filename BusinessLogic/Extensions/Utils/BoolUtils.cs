using System.Security.Cryptography;

namespace BusinessLogic.Extensions.Utils;

public static class BoolUtils
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        { return false; }
    }

    /// <summary>
    ///     This method is for verify/compare a password (not hashed) with a hashed password stored in database.<para/>
    ///     <see cref="VerifyPassword">VerifyPassword()</see> is a part of <see cref="StringUtils.HashPassword">HashPassword()</see>
    ///     due to <see cref="StringUtils.HashPassword">HashPassword()</see> return logic as a 3 parts string.
    /// </summary>
    /// <param name="password">non-hashed string (eg: "pass123")</param>
    /// <param name="storedPassword">hashed string (eg: "1000.123.c3j3h4jh")</param>
    /// <returns><c>true</c> if <paramref name="password"/> and <paramref name="storedPassword"/> are same and correct; otherwise, <c>false</c></returns>
    public static bool VerifyPassword(string password, string storedPassword)
    {
        // Seperate old hashed string to 3 part
        var parts = storedPassword.Split('.', 3);
        if (parts.Length != 3) return false;

        int iterations = int.Parse(parts[0]);
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] hash = Convert.FromBase64String(parts[2]); //Hashed data of storedPassword

        // Start hashing the new input (password) with same logic of storedPassword hashed data
        byte[] hashToVerify = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            StringUtils.HashAlgorithm,
            hash.Length
        );

        // Use FixedTimeEquals to prevent Timing Attack
        return CryptographicOperations.FixedTimeEquals(hash, hashToVerify);
    }

    /// <summary>
    ///     To check in <paramref name="fields"/> have atleast 1 input is null or white space.<para/>
    ///     Example: BoolUtils.HaveEmptyString("string1", "string2",...)
    /// </summary>
    /// <returns><c>true</c> if any in <paramref name="fields"/> is <b>IsNullOrWhiteSpace</b></returns>
    public static bool HaveEmptyString(params string?[] fields)
    {
        return fields.Any(string.IsNullOrWhiteSpace);
    }
}

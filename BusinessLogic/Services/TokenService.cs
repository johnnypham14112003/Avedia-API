using BusinessLogic.Interfaces;
using BusinessLogic.Models.StronglyTyped;
using DataAccess.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic.Services;

public class TokenService(JwtSetting jwtSetting) : ITokenService
{
    private readonly JwtSetting _jwtSetting = jwtSetting;
    private static readonly string encryptAlg = SecurityAlgorithms.HmacSha512;

    // =====================================================================
    public DateTime GetExpirationTimes() => DateTime.Now.AddDays(_jwtSetting.RefreshTokenExpirationDays);

    public string GenerateAccessToken(Account account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));
        var creds = new SigningCredentials(key, encryptAlg);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, account.UserName),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role),
            new Claim("Id", account.Id.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(_jwtSetting.AccessTokenExpirationMinutes),
            SigningCredentials = creds
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)),
            ValidateLifetime = false // false = Allow read expired token
        };

        var handler = new JsonWebTokenHandler();

        // ValidateToken return TokenValidationResult much safetier
        var result = await handler.ValidateTokenAsync(token, tokenValidationParameters);

        if (!result.IsValid)
            throw new SecurityTokenException("Invalid token");


        // Check encrypt algorithm (parse JsonWebToken instead JwtSecurityToken)
        if (result.SecurityToken is JsonWebToken jsonWebToken &&
            !jsonWebToken.Alg.Equals(encryptAlg, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token algorithm");
        }

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }
}

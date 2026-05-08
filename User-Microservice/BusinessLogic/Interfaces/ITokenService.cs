using DataAccess.Models;
using System.Security.Claims;

namespace BusinessLogic.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        ///     Create a jwt token base on <paramref name="account"/> info.
        /// </summary>
        string GenerateAccessToken(Account account);

        /// <returns>Random 64 byte numbers as string</returns>
        string GenerateRefreshToken();
        /// <summary>
        ///     This method return a predict DataTime value base on <b>"current system date:time"</b> plus(+) how long
        ///     does the <b>"refresh token life time"</b> (days) that is configured in .env
        /// </summary>
        /// <returns>A DateTime value = Current DateTime + Refresh token life time (days)</returns>
        DateTime GetExpirationTimes();
        Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token);
    }
}
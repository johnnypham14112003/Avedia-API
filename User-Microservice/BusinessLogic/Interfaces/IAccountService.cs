using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface IAccountService
{
    /// <summary>
    ///     This method is similar to login but instead of handling access token, it will just handle refresh token
    /// </summary>
    Task<ResultRs<AccountRs>> GetByPasswordAsync(AuthRq authRequest);
    Task<ResultRs<AccountRs>> RefreshTokenAsync(Guid id, string refreshToken);
    Task<ResultRs<bool>> RevokeRefreshTokenAsync(Guid accountId);
    Task<ResultRs<bool>> ChangePasswordAsync(string email, string newPassword);

    // ----------------------------< CRUD >----------------------------
    Task<ResultRs<bool>> CreateAccountAsync(AuthRq request);

    /// <summary>
    ///     If <paramref name="includeBadge"/> is <c>true</c>, return account by Id with badges as no tracking.
    /// </summary>
    Task<ResultRs<AccountRs>> GetAccountAsync(Guid id, bool includeBadge = false);

    /// <summary>
    ///     <![CDATA[ + Gender: 1-Boy | 2-Girl | 0/else-null]]>
    /// </summary>
    Task<ResultRs<PagedResult<AccountRs>>> GetAccountsPageAsync(PagingQueryRq<AccountQr> input);

    /// <summary>
    ///     If <paramref name="updateAll"/> is <c>false</c>, these field will not be updated:
    ///     <![CDATA[
    ///         + Account.IsVerified;
    ///         + Account.JoinedDate;
    ///         + Account.MeritPoint;
    ///         + Account.Role;
    ///         + Account.Password; (this is absolute can't updated, use method]]>
    ///         <see cref="ChangePasswordAsync(string, string)">ChangePasswordAsync()</see> instead)
    /// </summary>
    Task<ResultRs<bool>> UpdateAccountAsync(AccountRq request, bool updateAll = true);
    Task<ResultRs<bool>> DeleteAccountAsync(Guid id);
}

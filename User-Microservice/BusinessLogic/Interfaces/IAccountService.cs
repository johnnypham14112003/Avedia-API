using BusinessLogic.Models.Generic;
using BusinessLogic.Models.View.Request;
using BusinessLogic.Models.View.Request.Query;
using BusinessLogic.Models.View.Response;
using DataAccess.Models;

namespace BusinessLogic.Interfaces;

public interface IAccountService
{
    Task<ApiResult<AuthRs>> LoginByPasswordAsync(AuthRq authRequest);
    Task<ApiResult<AuthRs>> RefreshAccessToken(RefreshTokenRq request);
    Task<ApiResult<bool>> RevokeRefreshTokenAsync(Guid accountId);
    Task<ApiResult<bool>> ResetPasswordAsync(Account? alreadyQueried, Guid accountId, string newPassword);

    // ----------------------------< CRUD >----------------------------
    Task<ApiResult<bool>> CreateAccountAsync(AuthRq request);
    /// <summary>
    ///     If <paramref name="includeBadge"/> is <c>true</c>, return account by Id as no tracking.
    /// </summary>
    /// <returns><![CDATA[ApiResult < AccountRs >]]></returns>
    Task<ApiResult<AccountRs>> GetAccountAsync(Guid id, bool includeBadge = false);
    Task<ApiResult<PagedResult<AccountRs>>> GetAccountsPageAsync(PagingQueryRq<AccountQr> input);
    /// <summary>
    ///     If <paramref name="updateAll"/> is <c>false</c>, these field will not be updated:
    ///     <![CDATA[
    ///         + Account.IsVerified;
    ///         + Account.JoinedDate;
    ///         + Account.MeritPoint;
    ///         + Account.Role;
    ///     ]]>
    /// </summary>
    Task<ApiResult<bool>> UpdateAccountAsync(AccountRq request, bool updateAll = true);
    Task<ApiResult<bool>> DeleteAccountAsync(Guid id);
}

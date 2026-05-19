using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface IAccountService
{
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
    //Task<PagedResult<AccountRs>> GetAccountsPageAsync(PagingQueryRq<AccountQr> input);
    /// <summary>
    ///     If <paramref name="updateAll"/> is <c>false</c>, these field will not be updated:
    ///     <![CDATA[
    ///         + Account.IsVerified;
    ///         + Account.JoinedDate;
    ///         + Account.MeritPoint;
    ///         + Account.Role;
    ///         + Account.Password;
    ///     ]]>
    /// </summary>
    Task<ResultRs<bool>> UpdateAccountAsync(AccountRq request, bool updateAll = true);
    Task<ResultRs<bool>> DeleteAccountAsync(Guid id);
}

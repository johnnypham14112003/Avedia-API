using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces
{
    public interface IBadgeService
    {
        Task<ResultRs<bool>> AddBadgeToUserAsync(AccountBadgeRq request);
        Task<ResultRs<bool>> CreateBadgeAsync(BadgeRq request);
        Task<ResultRs<BadgeRs>> GetBadgeAsync(Guid id);
        Task<ResultRs<PagedResult<BadgeRs>>> GetBadgesPageAsync(PagingQueryRq<BadgeQr> input);
        Task<ResultRs<bool>> UpdateBadgeAsync(BadgeRq request);
        Task<ResultRs<bool>> DeleteBadgeAsync(Guid id);
        Task<ResultRs<bool>> DeletePermanentBadgeAsync(Guid id);
        Task<ResultRs<bool>> RemoveAllBadgesFromAccountAsync(Guid accountId);
    }
}
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface IContributionService
{
    Task<ResultRs<bool>> CreateContributionAsync(ContributionRq request);
    Task<ResultRs<ContributionRs>> GetContributionAsync(Guid contributionId);
    Task<ResultRs<PagedResult<ContributionRs>>> GetContributionsPageAsync(PagingQueryRq<ContributionQr> input);
    Task<ResultRs<bool>> UpdateContributionAsync(ContributionRq request);
    Task<ResultRs<bool>> DeleteContributionAsync(Guid id);
    Task<ResultRs<bool>> StatusContributionAsync(Guid contributionId, string newStatus);
    Task<ResultRs<bool>> ReviewContributionAsync(Guid contributionId, Guid approverId);
}
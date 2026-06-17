using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;

namespace AppGrpc.Services;

public class ContributionGrpcEndpoint(IContributionService contributionService) : ContributionGrpcService.ContributionGrpcServiceBase
{
    private readonly IContributionService _contributionService = contributionService;

    public override async Task<ContributionResponse> CreateContribution(ContributionRequest request, ServerCallContext context)
    {
        var result = await _contributionService.CreateContributionAsync(request.ContributionInfo.Adapt<ContributionRq>());
        return new ContributionResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<ContributionResponse> GetContribution(ContributionGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new ContributionResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _contributionService.GetContributionAsync(parsedId);

        return new ContributionResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<ContributionPageResponse> GetContributionsPage(ContributionPageRequest request, ServerCallContext context)
    {
        var pageQuery = request.PageQueryRequest;
        var advanceInput = request.AdvanceInput;

        // For parse DateOnly safety
        DateOnly? parsedFromDate = null;
        DateOnly? parsedToDate = null;
        Guid contributorIdQr = Guid.Empty;
        Guid targetIdQr = Guid.Empty;

        // Pre-check message before map to service method's param
        if (advanceInput != null)
        {
            if (!string.IsNullOrEmpty(advanceInput.FromDate))
                parsedFromDate = DateOnly.Parse(advanceInput.FromDate);

            if (!string.IsNullOrEmpty(advanceInput.ToDate))
                parsedToDate = DateOnly.Parse(advanceInput.ToDate);

            if (Guid.TryParse(advanceInput.ContributorId, out contributorIdQr) == false || contributorIdQr == Guid.Empty)
                return new ContributionPageResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "Contributor ID invalid!" } };

            if (Guid.TryParse(advanceInput.TargetId, out targetIdQr) == false || targetIdQr == Guid.Empty)
                return new ContributionPageResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "Target ID invalid!" } };
        }

        // ==========[ Mapping PROTO -> DTOs ]==========
        var queryInput = new PagingQueryRq<ContributionQr>
        {
            Keyword = pageQuery.Keyword,
            PageNumber = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            AdvanceInput = (advanceInput == null) ? null :
            new ContributionQr
            {
                ContributorId = contributorIdQr,
                TargetId = targetIdQr,
                FromDate = parsedFromDate,
                ToDate = parsedToDate,
                ByTypeDate = advanceInput.ByTypeDate,
                AdminApproved = advanceInput.AdminApproved,
                Status = advanceInput.Status
            }
        };

        // Call Repository Method to query in database
        var pagedResult = await _contributionService.GetContributionsPageAsync(queryInput);

        // ==========[ Mapping DATA -> PROTO ]==========
        // Create proto message [ContributionPageResponse](0)
        var response = new ContributionPageResponse
        {
            // Assign to proto message [ResultResponse](1)
            ResultResponse = pagedResult.Adapt<ResultResponse>()
        };

        if (pagedResult.Data != null)
        {
            // Create proto message [ContributionPagedResult](2*)
            var pagedDataProto = new ContributionPageResponse.Types.ContributionPagedResult
            {
                // Assign to proto message [BasePageResult](2.1)
                BasePageResult = new BasePageResult
                {
                    PageIndex = pagedResult.Data.PageIndex,
                    PageSize = pagedResult.Data.PageSize,
                    TotalCount = pagedResult.Data.TotalCount,
                    TotalPage = pagedResult.Data.TotalPage
                }
            };

            // Assign to proto message [repeated ContributionInfo](2.2)
            if (pagedResult.Data.DataList != null && pagedResult.Data.DataList.Any())
            {
                // ContributionRs:cs -> ContributionInfo:proto
                var mappedList = pagedResult.Data.DataList.Adapt<IEnumerable<ContributionInfo>>();
                pagedDataProto.DataList.AddRange(mappedList);
            }
            // Assign to proto message [ContributionPagedResult](2)
            response.PagedData = pagedDataProto;
        }

        return response;
    }

    public override async Task<ContributionResponse> UpdateContribution(ContributionRequest request, ServerCallContext context)
    {
        var result = await _contributionService.UpdateContributionAsync(request.ContributionInfo.Adapt<ContributionRq>());
        return new ContributionResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<ContributionResponse> DeleteContribution(ContributionGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new ContributionResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _contributionService.DeleteContributionAsync(parsedId);
        return new ContributionResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<ContributionResponse> StatusContribution(ContributionGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new ContributionResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        if (!request.HasNewStatus || string.IsNullOrEmpty(request.NewStatus))
            return new ContributionResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "Status is null!" } };

        var result = await _contributionService.StatusContributionAsync(parsedId, request.NewStatus);
        return new ContributionResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<ContributionResponse> ReviewContribution(ContributionGetter request, ServerCallContext context)
    {
        if (Guid.TryParse(request.Id, out var contriId) == false || contriId == Guid.Empty)
            return new ContributionResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "Contribution ID invalid!" } };

        if (Guid.TryParse(request.Id, out var approverId) == false || approverId == Guid.Empty)
            return new ContributionResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "Approver ID invalid!" } };

        var result = await _contributionService.ReviewContributionAsync(contriId, approverId);
        return new ContributionResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}

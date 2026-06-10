using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.Implements;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;

namespace AppGrpc.Services;

public class BadgeGrpcEndpoint(IBadgeService badgeService) : BadgeGrpcService.BadgeGrpcServiceBase
{
    private readonly IBadgeService _badgeService = badgeService;

    public override async Task<BadgeResponse> AddBadgeToUserAsync(AccountBadgeRequest request, ServerCallContext context)
    {
        var accIsParsed = Guid.TryParse(request.AccountId, out var accId);
        var badgeIsParsed = Guid.TryParse(request.BadgeId, out var badgeId);

        if (accIsParsed == false || badgeIsParsed == false || accId == Guid.Empty || badgeId == Guid.Empty)
            return new BadgeResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _badgeService.AddBadgeToUserAsync(request.Adapt<AccountBadgeRq>());
        return new BadgeResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<BadgeResponse> CreateBadgeAsync(BadgeRequest request, ServerCallContext context)
    {
        var result = await _badgeService.CreateBadgeAsync(request.BadgeInfo.Adapt<BadgeRq>());
        return new BadgeResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<BadgeResponse> GetBadgeAsync(BadgeGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new BadgeResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _badgeService.GetBadgeAsync(parsedId);

        return new BadgeResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<BadgePageResponse> GetBadgesPageAsync(BadgePageRequest request, ServerCallContext context)
    {
        // For parse DateOnly safety
        DateOnly? parsedFromDate = null;
        DateOnly? parsedToDate = null;

        var pageQuery = request.PageQueryRequest;
        var advanceInput = request.AdvanceInput;

        // Pre-check message before map to service method's param
        if (advanceInput != null)
        {
            if (!string.IsNullOrEmpty(advanceInput.FromDate))
                parsedFromDate = DateOnly.Parse(advanceInput.FromDate);

            if (!string.IsNullOrEmpty(advanceInput.ToDate))
                parsedToDate = DateOnly.Parse(advanceInput.ToDate);
        }
        // ==========[ Mapping PROTO -> DTOs ]==========
        var queryInput = new PagingQueryRq<BadgeQr>
        {
            Keyword = pageQuery.Keyword,
            PageNumber = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            AdvanceInput = (advanceInput == null) ? null :
            new BadgeQr
            {
                RareLevel = (short)advanceInput.RareLevel,
                FromDate = parsedFromDate,
                ToDate = parsedToDate,
                Status = advanceInput.Status
            }
        };

        // Call Repository Method to query in database
        var pagedResult = await _badgeService.GetBadgesPageAsync(queryInput);

        // ==========[ Mapping DATA -> PROTO ]==========
        // Create proto message [BadgePageResponse](0)
        var response = new BadgePageResponse
        {
            // Assign to proto message [ResultResponse](1)
            ResultResponse = pagedResult.Adapt<ResultResponse>()
        };

        if (pagedResult.Data != null)
        {
            // Create proto message [BadgePagedResult](2*)
            var pagedDataProto = new BadgePageResponse.Types.BadgePagedResult
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

            // Assign to proto message [repeated BadgeInfo](2.2)
            if (pagedResult.Data.DataList != null && pagedResult.Data.DataList.Any())
            {
                // BadgeRs:cs -> BadgeInfo:proto
                var mappedList = pagedResult.Data.DataList.Adapt<IEnumerable<BadgeInfo>>();
                pagedDataProto.DataList.AddRange(mappedList);
            }
            // Assign to proto message [BadgePagedResult](2)
            response.PagedData = pagedDataProto;
        }

        return response;
    }

    public override async Task<BadgeResponse> UpdateBadgeAsync(BadgeRequest request, ServerCallContext context)
    {
        var result = await _badgeService.UpdateBadgeAsync(request.BadgeInfo.Adapt<BadgeRq>());
        return new BadgeResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<BadgeResponse> DeleteBadgeAsync(BadgeGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new BadgeResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _badgeService.DeleteBadgeAsync(parsedId);
        return new BadgeResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<BadgeResponse> DeletePermanentBadgeAsync(BadgeGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new BadgeResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _badgeService.DeletePermanentBadgeAsync(parsedId);
        return new BadgeResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<BadgeResponse> RemoveAllBadgesFromAccountAsync(BadgeGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new BadgeResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _badgeService.DeletePermanentBadgeAsync(parsedId);
        return new BadgeResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}

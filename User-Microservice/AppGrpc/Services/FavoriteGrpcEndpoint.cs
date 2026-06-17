using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;

namespace AppGrpc.Services;

public class FavoriteGrpcEndpoint(IFavoriteService favoriteService) : FavoriteGrpcService.FavoriteGrpcServiceBase
{
    private readonly IFavoriteService _favoriteService = favoriteService;

    public override async Task<FavoriteResponse> CheckUserFavorited(FavoriteRequest request, ServerCallContext context)
    {
        var result = await _favoriteService.CheckUserFavoritedAsync(request.FavoriteInfo.Adapt<FavoriteRq>());

        return new FavoriteResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
    public override async Task<FavoriteResponse> CountTargetFavorite(FavoriteRequest request, ServerCallContext context)
    {
        var favorite = request.FavoriteInfo;
        var isParsed = Guid.TryParse(favorite.TargetId, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new FavoriteResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _favoriteService.CountTargetFavoriteAsync(favorite.TargetType, parsedId);
        return new FavoriteResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            FavoriteNumber = result.Data
        };
    }
    public override async Task<FavoritePageResponse> GetUserFavorites(FavoritePageRequest request, ServerCallContext context)
    {
        var pageQuery = request.PageQueryRequest;
        var advanceInput = request.AdvanceInput;

        _ = Guid.TryParse(advanceInput.AccountId, out var AccountId);
        _ = Guid.TryParse(advanceInput.TargetId, out var targetId);

        var queryInput = new PagingQueryRq<FavoriteRq>
        {
            Keyword = pageQuery.Keyword,
            PageNumber = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            AdvanceInput = (advanceInput == null) ? null :
            new FavoriteRq
            {
                AccountId = AccountId,
                TargetId = targetId,
                TargetType = advanceInput.TargetType
            }
        };

        var pagedResult = await _favoriteService.GetUserFavoritesAsync(queryInput);
        var response = new FavoritePageResponse
        {
            //[ResultResponse](1)
            ResultResponse = pagedResult.Adapt<ResultResponse>()
        };

        if (pagedResult.Data != null)
        {
            // [AccountPagedResult](2)
            var pagedDataProto = new FavoritePageResponse.Types.FavoritePagedResult
            {
                // [BasePageResult](2.1)
                BasePageResult = new BasePageResult
                {
                    PageIndex = pagedResult.Data.PageIndex,
                    PageSize = pagedResult.Data.PageSize,
                    TotalCount = pagedResult.Data.TotalCount,
                    TotalPage = pagedResult.Data.TotalPage
                }
            };

            // [repeated FavoriteInfo](2.2)
            if (pagedResult.Data.DataList != null && pagedResult.Data.DataList.Any())
            {
                // FavoriteRs:cs -> FavoriteInfo:proto
                var mappedList = pagedResult.Data.DataList.Adapt<IEnumerable<FavoriteInfo>>();
                pagedDataProto.DataList.AddRange(mappedList);
            }
            // [FavoritePagedResult](2)
            response.PagedData = pagedDataProto;
        }

        return response;
    }

    public override async Task<FavoriteResponse> ToggleFavorite(FavoriteRequest request, ServerCallContext context)
    {
        var favorite = request.FavoriteInfo;
        var result = await _favoriteService.ToggleFavoriteAsync(favorite.Adapt<FavoriteRq>());
        return new FavoriteResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}

using GraphQLGateway.DTOs.Generic;
using GraphQLGateway.DTOs.Request;
using GraphQLGateway.DTOs.Response;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Favorites;

[ExtendObjectType(OperationTypeNames.Query)]
public class FavoriteQueries
{
    public async Task<PagedResult<FavoriteRs>> GetUserFavoritesAsync(PagingQueryRq<FavoriteRq?> input,
        FavoriteGrpcService.FavoriteGrpcServiceClient grpcClient)
    {
        // Basic Validate
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 10;

        // Create gRPC input
        var request = new FavoritePageRequest
        {
            PageQueryRequest = new PageQueryRequest
            {
                Keyword = input.Keyword ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            AdvanceInput = input.AdvanceInput.Adapt<FavoriteQuery>()
        };

        // Call gRPC
        var response = await grpcClient.GetUserFavoritesAsync(request);
        if (!response.ResultResponse.Success)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return new PagedResult<FavoriteRs>
        {
            PageIndex = response.PagedData.BasePageResult.PageIndex,
            PageSize = response.PagedData.BasePageResult.PageSize,
            TotalCount = response.PagedData.BasePageResult.TotalCount,
            TotalPage = response.PagedData.BasePageResult.TotalPage,
            // Map gRPC message (FavoriteInfo) to GraphQL model (FavoriteRs)
            DataList = [.. response.PagedData.DataList.Select(x => x.Adapt<FavoriteRs>())]
        };
    }
}

using GraphQLGateway.DTOs.Generic;
using GraphQLGateway.DTOs.Request.Query;
using GraphQLGateway.DTOs.Response;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Badges;

[ExtendObjectType(OperationTypeNames.Query)]
public class BadgeQueries
{
    public async Task<BadgeRs> GetBadgeAsync(Guid id,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new BadgeGetter
        {
            Id = id.ToString()
        };

        // Call gRPC
        var response = await grpcClient.GetBadgeAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.BadgeInfo.Adapt<BadgeRs>();
    }

    public async Task<PagedResult<BadgeRs>> GetBadgesPageAsync(PagingQueryRq<BadgeQr?> input,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Basic Validate
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 10;

        // Create gRPC input
        var request = new BadgePageRequest
        {
            PageQueryRequest = new PageQueryRequest
            {
                Keyword = input.Keyword ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            AdvanceInput = input.AdvanceInput.Adapt<BadgeQuery>()
        };

        // Call gRPC
        var response = await grpcClient.GetBadgesPageAsync(request);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return new PagedResult<BadgeRs>
        {
            PageIndex = response.PagedData.BasePageResult.PageIndex,
            PageSize = response.PagedData.BasePageResult.PageSize,
            TotalCount = response.PagedData.BasePageResult.TotalCount,
            TotalPage = response.PagedData.BasePageResult.TotalPage,
            // Map gRPC message (BadgeInfo) to GraphQL model (BadgeRs)
            DataList = [.. response.PagedData.DataList.Select(x => x.Adapt<BadgeRs>())]
        };
    }
}

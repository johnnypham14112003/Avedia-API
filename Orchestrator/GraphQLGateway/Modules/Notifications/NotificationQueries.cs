using GraphQLGateway.DTOs.Generic;
using GraphQLGateway.DTOs.Request.Query;
using GraphQLGateway.DTOs.Response;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Notifications;

[ExtendObjectType(OperationTypeNames.Query)]
public class NotificationQueries
{
    public async Task<PagedResult<NotificationRs>> GetContributionsPageAsync(PagingQueryRq<NotificationQr?>
         query,
        NotificationGrpcService.NotificationGrpcServiceClient grpcClient)
    {
        // Basic Validate
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        // Create gRPC input
        var request = new NotificationPageRequest
        {
            PageQueryRequest = new PageQueryRequest
            {
                Keyword = query.Keyword ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            AdvanceInput = query.AdvanceInput.Adapt<NotificationQuery>()
        };

        // Call gRPC
        var response = await grpcClient.GetMyNotificationsAsync(request);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return new PagedResult<NotificationRs>
        {
            PageIndex = response.PagedData.BasePageResult.PageIndex,
            PageSize = response.PagedData.BasePageResult.PageSize,
            TotalCount = response.PagedData.BasePageResult.TotalCount,
            TotalPage = response.PagedData.BasePageResult.TotalPage,
            // Map gRPC message (ContributionInfo) to GraphQL model (ContributionRs)
            DataList = [.. response.PagedData.DataList.Select(x => x.Adapt<NotificationRs>())]
        };
    }
}
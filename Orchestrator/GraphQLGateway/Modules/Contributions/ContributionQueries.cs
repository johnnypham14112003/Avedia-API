using GraphQLGateway.DTOs.Generic;
using GraphQLGateway.DTOs.Request.Query;
using GraphQLGateway.DTOs.Response;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Contributions;

[ExtendObjectType(OperationTypeNames.Query)]
public class ContributionQueries
{
    public async Task<ContributionRs> GetContributionAsync(Guid contributionId,
        ContributionGrpcService.ContributionGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new ContributionGetter
        {
            Id = contributionId.ToString()
        };

        // Call gRPC
        var response = await grpcClient.GetContributionAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ContributionInfo.Adapt<ContributionRs>();
    }

    public async Task<PagedResult<ContributionRs>> GetContributionsPageAsync(PagingQueryRq<ContributionQr?>
         query,
        ContributionGrpcService.ContributionGrpcServiceClient grpcClient)
    {
        // Basic Validate
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        // Create gRPC input
        var request = new ContributionPageRequest
        {
            PageQueryRequest = new PageQueryRequest
            {
                Keyword = query.Keyword ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            AdvanceInput = query.AdvanceInput.Adapt<ContributionQuery>()
        };

        // Call gRPC
        var response = await grpcClient.GetContributionsPageAsync(request);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return new PagedResult<ContributionRs>
        {
            PageIndex = response.PagedData.BasePageResult.PageIndex,
            PageSize = response.PagedData.BasePageResult.PageSize,
            TotalCount = response.PagedData.BasePageResult.TotalCount,
            TotalPage = response.PagedData.BasePageResult.TotalPage,
            // Map gRPC message (ContributionInfo) to GraphQL model (ContributionRs)
            DataList = [.. response.PagedData.DataList.Select(x => x.Adapt<ContributionRs>())]
        };
    }
}
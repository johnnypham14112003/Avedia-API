using GraphQLGateway.DTOs.Generic;
using GraphQLGateway.DTOs.Request;
using GraphQLGateway.DTOs.Request.Query;
using GraphQLGateway.DTOs.Response;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Accounts;

[ExtendObjectType(OperationTypeNames.Query)]
public class AccountQueries
{
    public async Task<AccountRs> GetByPasswordAsync(AuthRq request,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.GetByPasswordAsync(request.Adapt<AuthRequest>());
        if (!response.ResultResponse.Success)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.AccountInfo.Adapt<AccountRs>();
    }

    public async Task<AccountRs> RefreshTokenAsync(AuthRq request,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.GetByPasswordAsync(request.Adapt<AuthRequest>());
        if (!response.ResultResponse.Success)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.AccountInfo.Adapt<AccountRs>();
    }

    public async Task<AccountRs> GetAccountAsync(Guid id, bool? includeBadge,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var request = new AccountGetter
        {
            Id = id.ToString(),
            IncludeBadge = includeBadge ?? false
        };

        // Call gRPC
        var response = await grpcClient.GetAccountAsync(request);
        if (!response.ResultResponse.Success)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);


        // Map AccountInfo -> AccountRs
        var result = response.AccountInfo.Adapt<AccountRs>();

        if (response.AccountBadgeResponse is not null && response.AccountBadgeResponse.Count != 0)
        {
            result.AccountBadges = [.. response.AccountBadgeResponse.Select(badgeResp => new AccountBadgeRs
            {
                // Parse gRPC string to Guid
                AccountId = Guid.Parse(badgeResp.AccountBadgeInfo.AccountId),
                BadgeId = Guid.Parse(badgeResp.AccountBadgeInfo.BadgeId),

                // Parse gRPC string to DateOnly
                AwardedDate = DateOnly.Parse(badgeResp.AccountBadgeInfo.AwardedDate),

                // Map BadgeInfo to BadgeRs
                Badge = badgeResp.BadgeInfo?.Adapt<BadgeRs>(),
            })];
        }

        return result;
    }

    public async Task<PagedResult<AccountRs>> GetAccountsAsync(string? keyword, int pageNumber, int pageSize,
        AccountQr? accountQr,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Basic Validate
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        // Create gRPC input
        var request = new AccountPageRequest
        {
            PageQueryRequest = new PageQueryRequest
            {
                Keyword = keyword ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            AdvanceInput = accountQr.Adapt<AccountQuery>()
        };

        // Call gRPC
        var response = await grpcClient.GetAccountListAsync(request);
        if (!response.ResultResponse.Success)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);


        return new PagedResult<AccountRs>
        {
            PageIndex = response.PagedData.BasePageResult.PageIndex,
            PageSize = response.PagedData.BasePageResult.PageSize,
            TotalCount = response.PagedData.BasePageResult.TotalCount,
            TotalPage = response.PagedData.BasePageResult.TotalPage,
            // Map gRPC message (AccountInfo) to GraphQL model (AccountRs)
            DataList = [.. response.PagedData.DataList.Select(x => x.Adapt<AccountRs>())]
        };
    }
}
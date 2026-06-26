using GraphQLGateway.DTOs.Request;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Badges;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class BadgeMutations
{
    public async Task<bool> CreateBadgeAsync(BadgeRq request,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var badgeRequest = new BadgeRequest
        {
            BadgeInfo = request.Adapt<BadgeInfo>()
        };

        // Call gRPC
        var response = await grpcClient.CreateBadgeAsync(badgeRequest);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> AddBadgeToUserAsync(AccountBadgeRq request,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.AddBadgeToUserAsync(request.Adapt<AccountBadgeRequest>());
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> UpdateBadgeAsync(BadgeRq request,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var badgeRequest = new BadgeRequest
        {
            BadgeInfo = request.Adapt<BadgeInfo>()
        };

        // Call gRPC
        var response = await grpcClient.UpdateBadgeAsync(badgeRequest);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> DeleteBadgeAsync(Guid id,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new BadgeGetter
        {
            Id = id.ToString()
        };

        // Call gRPC
        var response = await grpcClient.DeleteBadgeAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }
    public async Task<bool> DeletePermanentBadgeAsync(Guid id,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new BadgeGetter
        {
            Id = id.ToString()
        };

        // Call gRPC
        var response = await grpcClient.DeletePermanentBadgeAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }
    public async Task<bool> RemoveAllBadgesFromAccountAsync(Guid id,
        BadgeGrpcService.BadgeGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new BadgeGetter
        {
            Id = id.ToString()
        };

        // Call gRPC
        var response = await grpcClient.RemoveAllBadgesFromAccountAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }
}

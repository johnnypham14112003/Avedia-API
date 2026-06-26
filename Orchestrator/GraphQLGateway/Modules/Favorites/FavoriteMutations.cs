using GraphQLGateway.DTOs.Request;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Favorites;

[ExtendObjectType(OperationTypeNames.Query)]
public class FavoriteMutations
{
    public async Task<bool> CheckUserFavoritedAsync(FavoriteRq request,
        FavoriteGrpcService.FavoriteGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new FavoriteRequest
        {
            FavoriteInfo = request.Adapt<FavoriteInfo>()
        };

        // Call gRPC
        var response = await grpcClient.CheckUserFavoritedAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<int> CountTargetFavoriteAsync(FavoriteRq request,
        FavoriteGrpcService.FavoriteGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new FavoriteRequest
        {
            FavoriteInfo = request.Adapt<FavoriteInfo>()
        };

        // Call gRPC
        var response = await grpcClient.CountTargetFavoriteAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.FavoriteNumber;
    }

    public async Task<bool> ToggleFavoriteAsync(FavoriteRq request,
        FavoriteGrpcService.FavoriteGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new FavoriteRequest
        {
            FavoriteInfo = request.Adapt<FavoriteInfo>()
        };

        // Call gRPC
        var response = await grpcClient.ToggleFavoriteAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }
}

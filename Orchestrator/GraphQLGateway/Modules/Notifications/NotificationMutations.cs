using GraphQLGateway.DTOs.Request;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Notifications;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class NotificationMutations
{
    public async Task<bool> CreateGlobalNotificationAsync(NotificationRq request,
        NotificationGrpcService.NotificationGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new NotificationRequest
        {
            NotificationInfo = request.Adapt<NotificationInfo>()
        };

        // Call gRPC
        var response = await grpcClient.CreateGlobalNotificationAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> CreatePersonalNotificationAsync(NotificationRq request,
        NotificationGrpcService.NotificationGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new NotificationRequest
        {
            NotificationInfo = request.Adapt<NotificationInfo>()
        };

        // Call gRPC
        var response = await grpcClient.CreatePersonalNotificationAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> MarkAsReadAsync(Guid accountId, Guid notificationId,
        NotificationGrpcService.NotificationGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new NotificationGetter
        {
            AccountId = accountId.ToString(),
            NotificationId = notificationId.ToString()
        };

        // Call gRPC
        var response = await grpcClient.MarkAsReadAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid accountId,
        NotificationGrpcService.NotificationGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new NotificationGetter
        {
            AccountId = accountId.ToString()
        };

        // Call gRPC
        var response = await grpcClient.MarkAllAsReadAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId,
        NotificationGrpcService.NotificationGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new NotificationGetter
        {
            NotificationId = notificationId.ToString()
        };

        // Call gRPC
        var response = await grpcClient.DeleteNotificationAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }
}

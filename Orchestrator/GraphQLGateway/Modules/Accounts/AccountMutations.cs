using GraphQLGateway.DTOs.Request;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Accounts;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AccountMutations
{
    public async Task<bool> RevokeRefreshTokenAsync(Guid accountId,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.RevokeRefreshTokenAsync(new AccountGetter { Id = accountId.ToString() });
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> ChangePasswordAsync(AuthRq request,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.ChangePasswordAsync(request.Adapt<AuthRequest>());
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> CreateAccountAsync(AuthRq request,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.CreateAccountAsync(request.Adapt<AuthRequest>());
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }


    public async Task<bool> UpdateAccountAsync(AccountRq request,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.UpdateAccountAsync(request.Adapt<AccountRequest>());
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> DeleteAccountAsync(Guid id,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.DeleteAccountAsync(new AccountGetter { Id = id.ToString() });
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> DeletePermanentAccountAsync(Guid id,
        AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        // Call gRPC
        var response = await grpcClient.DeletePermanentAccountAsync(new AccountGetter { Id = id.ToString() });
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }
}

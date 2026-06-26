using GraphQLGateway.DTOs.Request;
using GraphQLGateway.Protos;
using Mapster;

namespace GraphQLGateway.Modules.Contributions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class ContributionMutations
{
    public async Task<bool> CreateContributionAsync(ContributionRq request,
        ContributionGrpcService.ContributionGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new ContributionRequest
        {
            ContributionInfo = request.Adapt<ContributionInfo>()
        };

        // Call gRPC
        var response = await grpcClient.CreateContributionAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> UpdateContributionAsync(ContributionRq request,
        ContributionGrpcService.ContributionGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new ContributionRequest
        {
            ContributionInfo = request.Adapt<ContributionInfo>()
        };

        // Call gRPC
        var response = await grpcClient.UpdateContributionAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> DeleteContributionAsync(Guid contributionId,
        ContributionGrpcService.ContributionGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new ContributionGetter
        {
            Id = contributionId.ToString()
        };

        // Call gRPC
        var response = await grpcClient.DeleteContributionAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> StatusContributionAsync(Guid contributionId, string newStatus,
        ContributionGrpcService.ContributionGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new ContributionGetter
        {
            Id = contributionId.ToString(),
            NewStatus = newStatus
        };

        // Call gRPC
        var response = await grpcClient.StatusContributionAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

    public async Task<bool> ReviewContributionAsync(Guid contributionId, Guid approverId,
        ContributionGrpcService.ContributionGrpcServiceClient grpcClient)
    {
        // Create gRPC input
        var input = new ContributionGetter
        {
            Id = contributionId.ToString(),
            ApproverId = approverId.ToString(),
        };

        // Call gRPC
        var response = await grpcClient.ReviewContributionAsync(input);
        if (response.ResultResponse.HttpCode != 200)
            throw new GraphQLException(response.ResultResponse.ErrorMessage);

        return response.ResultResponse.Success;
    }

}
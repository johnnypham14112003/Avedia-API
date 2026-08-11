using Grpc.Core;
using Grpc.Core.Interceptors;

namespace AppGrpc.Interceptors;

public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning("gRPC Error: {StatusCode} - {Message}", ex.Status.StatusCode, ex.Status.Detail);

            throw;
        }
        // Catch all other error (Crash DB, NullReference, v.v.)
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled system error occurred: {Message}", ex.Message);

            // Return Error Internal (500) without crash server
            throw new RpcException(new Status(StatusCode.Internal, "There are internal system error in User-Microservice."));
        }
    }
}
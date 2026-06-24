using GraphQLGateway.Protos;
using Microsoft.AspNetCore.RateLimiting;
using GraphQLGateway.Modules.Accounts;
using GraphQLGateway.Modules.Badges;
using GraphQLGateway.Modules.Contributions;

namespace GraphQLGateway;

public static class DIContainer
{
    public static IServiceCollection ServicesRegister(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigGrpc(services);
        ConfigHotChocolate(services);

        ConfigRateLimit(services);
        ConfigCORS(services);

        // Final
        services.AddOpenApi();

        return services;
    }

    private static IServiceCollection ConfigGrpc(IServiceCollection services)
    {
        var accountUri = Environment.GetEnvironmentVariable("ACCOUNT_GRPC_URI");
        services.AddGrpcClient<AccountGrpcService.AccountGrpcServiceClient>(options =>
        {
            options.Address = new Uri("https://localhost:5001");
        });

        return services;
    }

    private static IServiceCollection ConfigHotChocolate(IServiceCollection services)
    {
        services.AddGraphQLServer()
            .AddQueryType(d => d.Name(OperationTypeNames.Query))
            .AddMutationType(d => d.Name(OperationTypeNames.Mutation))
            .AddTypeExtension<AccountQueries>()
            .AddTypeExtension<AccountMutations>()
            .AddTypeExtension<BadgeQueries>()
            .AddTypeExtension<BadgeMutations>()
            .AddTypeExtension<ContributionQueries>()
            .AddTypeExtension<ContributionMutations>();
        // .AddTypeExtension<ProductQueries>() ..
        return services;
    }

    private static IServiceCollection ConfigRateLimit(IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("FiftyOne", opt =>
            {
                opt.PermitLimit = 50; // Max 50 request
                opt.Window = TimeSpan.FromMinutes(1); // Within 1min
                opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0; // If reach over limit, deny directly (return http 429 Too Many Requests)
            });

            // Auto return 429 instead default 503.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }

    private static IServiceCollection ConfigCORS(IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            options.AddPolicy(name: "AVECORS", policy =>
            {
                // List domains allowed to call API
                policy.WithOrigins("https://google.com",
                                   "http://localhost:3000") // Allow localhost to test React, Mobile...
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
    }
}

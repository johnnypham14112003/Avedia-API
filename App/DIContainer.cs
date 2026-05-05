using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace App;

public static class DIContainer
{
    public static IServiceCollection ServicesRegister(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigRateLimit(services);

        InjectDbContext(services, configuration);
        InjectServiceClasses(services);
        InjectRepositoryClasses(services);

        ConfigJsonOption(services);
        ConfigCORS(services);

        return services;
    }

    // --------------------------------------------------------------------------------------------------------------
    private static IServiceCollection InjectDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_PG_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is missing. Set DB_PG_CONNECTION_STRING or ConnectionStrings:DefaultConnection.");

        return services.AddDbContext<AVEDbContext>(options => options.UseNpgsql(connectionString));
    }

    private static IServiceCollection InjectServiceClasses(IServiceCollection services)
    { return services; }

    private static IServiceCollection InjectRepositoryClasses(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        //services.AddScoped<IExampleRepository, ExampleRepository>();
        return services;
    }

    /// <summary>
    /// Limit how much request for an IP/user call to this server.
    /// To use this, register in program.cs with this name "FiftyOne" (eg: app.UseRateLimiter(); app.MapControllers().RequireRateLimiting("FiftyOne"); )
    /// </summary>
    /// <returns>limit: 50 | in mins: 1</returns>
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

    /// <summary>
    /// Config Json Option for Serialize or Deserialize.
    /// Current Include:
    /// + [kebab-case naming]
    /// + [ReferenceLoopHandling]
    /// </summary>
    private static IServiceCollection ConfigJsonOption(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            // Replace [Serialization with Reference of Newtonsoft] in older .NET with [System.Text.Json] intergrated in .NET 10
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

            // Convert name Property of Model (Input/Output Body) [ONLY AFFECT TO JSON BODY (Payload)]
            // Replace [custom IOutboundParameterTransformer/using Newtonsoft] in older .NET with [System.Text.Json] intergrated in .NET 10
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;

            // This line config for API return Dictionary<string, object>, it convert key string into kebab
            //options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.KebabCaseLower;
        });

        // If want to kebab on route (eg:GET /api/notifications?account-id=123)
        // => Map by handmade on controller
        //public IActionResult GetNotifications([FromQuery(Name = "account-id")] Guid accountId){...}
        return services;
    }

    /// <summary>
    /// Allow which domain can request to this server.
    /// To use this, register in program.cs with this name "AVECORS" (eg: app.UseCors("AVECORS"); )
    /// </summary>
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

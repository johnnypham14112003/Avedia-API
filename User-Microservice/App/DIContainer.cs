using BusinessLogic.Implements;
using BusinessLogic.Interfaces;
using BusinessLogic.Models.StronglyTyped;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

        ConfigJWTAuth(services, configuration);
        ConfigControllerRoute(services);
        ConfigJsonOption(services);
        ConfigCORS(services);

        return services;
    }

    // --------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     This method register the <see cref="AVEDbContext">AVEDbContext</see> to app life cycle handling for using
    ///     in <c>Dependency Injection</c> with the connection string read from <c>.env</c> then <c>appsettings.json</c><para/>
    ///     DbContext is a class that help O/RM logic from C# to SQL then connect and execute to database.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection InjectDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_PG_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is missing. Set DB_PG_CONNECTION_STRING or ConnectionStrings:DefaultConnection.");

        return services.AddDbContext<AVEDbContext>(options => options.UseNpgsql(connectionString));
    }

    private static IServiceCollection InjectServiceClasses(IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }

    private static IServiceCollection InjectRepositoryClasses(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        return services;
    }

    /// <summary>
    ///     Limit how much request for an IP/user call to this server.<para/>
    ///     To use this, register in <c>program.cs</c> with this name <b>"FiftyOne"</b>.<para/>
    ///     <![CDATA[Example:
    ///         app.UseRateLimiter();
    ///         app.MapControllers().RequireRateLimiting("FiftyOne");
    ///     ]]>
    /// </summary>
    /// <returns>limit: 50 requests | in mins: 1</returns>
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
    ///     Config Jwt Authentication with editable variables in .env > appsettings.json with keyword "JWT".<para/>
    ///     Also to get these variables value, inject the <see cref="BusinessLogic.Models.StronglyTyped.JwtSetting">JwtSetting</see>
    ///     to constructor and use it directly as strong-typed model.
    ///     <![CDATA[
    ///     Example: (Base-role Authorize)
    ///         [Authorize]         // For all role
    ///             or
    ///         [Authorize(Roles = "Admin, Staff")] // Only these roles allows
    ///         method in controller...
    ///     ]]>
    /// </summary>
    /// <returns>Registered "builder.Services.AddAuthorization();" as "return services.AddAuthorization();"</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection ConfigJWTAuth(IServiceCollection services, IConfiguration configuration)
    {
        var exMin = int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES"), out var m)
            ? m : int.TryParse(configuration["Jwt:AccessTokenExpirationMinutes"], out var cm) ? cm : 15;
        var exDay = int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS"), out var d)
            ? d : int.TryParse(configuration["Jwt:RefreshTokenExpirationDays"], out var cd) ? cd : 7;

        // StrongTyped Model
        var jwtOps = new JwtSetting
        {
            // Priority: environment variables > appsettings > defaults/null.
            Key = Environment.GetEnvironmentVariable("JWT_KEY") ?? configuration["Jwt:Key"] ?? string.Empty,
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? configuration["Jwt:Issuer"] ?? string.Empty,
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? configuration["Jwt:Audience"] ?? string.Empty,
            AccessTokenExpirationMinutes = exMin,
            RefreshTokenExpirationDays = exDay
        };

        if (string.IsNullOrWhiteSpace(jwtOps.Key) || string.IsNullOrWhiteSpace(jwtOps.Issuer) || string.IsNullOrWhiteSpace(jwtOps.Audience))
        {
            throw new InvalidOperationException("JWT config strings are missing. Set JWT_KEY/JWT_ISSUER/JWT_AUDIENCE or Jwt section in appsettings.");
        }

        // Register JwtSetting as a singleton
        services.AddSingleton(jwtOps);

        // Config Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOps.Issuer,
                ValidAudience = jwtOps.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOps.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Config Authorization
        return services.AddAuthorization();
    }
    private static IServiceCollection ConfigControllerRoute(IServiceCollection services)
    {
        return services.AddRouting(options => options.LowercaseUrls = true);
    }

    /// <summary>
    ///     Config Json Option for Serialize or Deserialize.<para/>
    ///     Current Include:<para/>
    ///     + <c>kebab-case</c> naming<para/>
    ///     + <c>ReferenceLoopHandling</c>
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
    ///     Allow which domain can request to this server. Domains be register in <c>.env</c> file seperate with "," (comma)<para/>
    ///     To use this, register in <c>program.cs</c> with this name <b>"AVECORS"</b> (eg: <b>app.UseCors("AVECORS");</b> ).
    /// </summary>
    private static IServiceCollection ConfigCORS(IServiceCollection services)
    {
        var corsOriginsVar = Environment.GetEnvironmentVariable("ALLOWED_CORS_ORIGINS");

        string[] allowedOrigins = string.IsNullOrWhiteSpace(corsOriginsVar)
            ? Array.Empty<string>() : corsOriginsVar.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return services.AddCors(options =>
        {
            options.AddPolicy(name: "AVECORS", policy =>
            {
                // List domains allowed to call API
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });
    }
}

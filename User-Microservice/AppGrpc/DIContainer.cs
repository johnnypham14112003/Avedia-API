using AppGrpc.Interceptors;
using BusinessLogic.Implements;

//using BusinessLogic.Implements;
using BusinessLogic.Interfaces;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppGrpc;

public static class DIContainer
{
    public static IServiceCollection ServicesRegister(this IServiceCollection services, IConfiguration configuration)
    {
        InjectDbContext(services, configuration);
        InjectServiceClasses(services);
        InjectRepositoryClasses(services);

        ConfigGrpc(services);

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
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }

    private static IServiceCollection InjectRepositoryClasses(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        return services;
    }

    private static IServiceCollection ConfigGrpc(IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        return services;
    }
}

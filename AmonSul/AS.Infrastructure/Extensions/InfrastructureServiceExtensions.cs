using AS.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCorsConfiguration();
        services.AddDatabaseContextConfiguration(configuration);
        services.AddRepositoryServices(configuration);
        services.AddHangfireConfiguration(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddSingleton<Utilidades>();

        return services;
    }
}
using AS.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Infrastructure.Configurations;

public static class DataBaseContextConfigurations
{
    public static IServiceCollection AddDatabaseContextConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuramos las migraciones para que estén en AS.Infrastructure
        string migrationsAssembly = typeof(DataBaseContextConfigurations).Assembly.FullName!;

        services.AddDbContext<DbamonsulContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("MyASPString"),
                b => b.MigrationsAssembly(migrationsAssembly)),
                ServiceLifetime.Scoped);

        return services;
    }

}

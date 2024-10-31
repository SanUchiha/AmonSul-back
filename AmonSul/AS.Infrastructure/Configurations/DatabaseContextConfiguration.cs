using AS.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Infrastructure.Configurations;

public static class DataBaseContextConfigurations
{
    public static IServiceCollection AddDatabaseContextConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        string? assembly = typeof(DbamonsulContext).Assembly.FullName;

        services.AddDbContext<DbamonsulContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("MyASPString"),
                b => b.MigrationsAssembly(assembly)),
                ServiceLifetime.Transient);

        return services;
    }

}

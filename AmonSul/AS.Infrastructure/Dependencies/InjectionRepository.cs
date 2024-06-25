using AS.Domain.Models;
using AS.Infrastructure.Repositories;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Infrastructure.Dependencies
{
    public static class InjectionRepository
    {
        public static IServiceCollection AddInjectionInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(DbamonsulContext).Assembly.FullName;

            services.AddDbContext<DbamonsulContext>(
                options => options.UseSqlServer(
                    configuration.GetConnectionString("SQLServerSW"), b => b.MigrationsAssembly(assembly)), ServiceLifetime.Transient);

            // Configurar patrones de diseño
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAccountRepository, AccountRepository>();

            return services;
        }
    }
}

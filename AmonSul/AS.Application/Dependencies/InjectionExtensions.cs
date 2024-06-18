using AS.Application.Interfaces;
using AS.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AS.Application.Dependencies
{
    public static class InjectionExtensions
    {
        public static IServiceCollection AddInjectionApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddScoped<IUsuarioApplication, UsuarioApplication>();
            services.AddScoped<ILoginApplication, LoginApplication>();

            return services;
        }
    }
}

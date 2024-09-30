using AS.Application.Interfaces;
using AS.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Application.Dependencies
{
    public static class InjectionExtensions
    {
        public static IServiceCollection AddInjectionApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);

            services.AddScoped<IUsuarioApplication, UsuarioApplication>();
            services.AddScoped<ILoginApplication, LoginApplication>();
            services.AddScoped<IFaccionApplication, FaccionApplication>();
            services.AddScoped<ITorneoApplication, TorneoApplication>();
            services.AddScoped<IPartidaAmistosaApplication, PartidaAmistosaApplication>();
            services.AddScoped<IEloApplication, EloApplication>();
            services.AddScoped<IInscripcionApplication, InscripcionApplication>();
            services.AddScoped<IListaApplication, ListaApplication>();
            services.AddScoped<IPartidaTorneoApplication, PartidaTorneoApplication>();
            services.AddScoped<IGanadorApplication, GanadorApplication>();

            return services;
        }
    }
}

using Microsoft.OpenApi.Models;

namespace AS.API.Configurations;

public static class SwaggerConfigurations
{
    public static IServiceCollection AddSwaggerConfigurations(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API Amon Sûl",
                Version = "v1",
                Description = "Swagger de la API de Amon Sûl",
                Contact = new OpenApiContact
                {
                    Name = "Jose Antonio Sanchez Molina",
                    Email = "jose.a.sanchez.molina@gmail.com",
                }
            });
        });

        return services;
    }
}

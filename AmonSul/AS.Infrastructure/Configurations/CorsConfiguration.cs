using Microsoft.Extensions.DependencyInjection;

namespace AS.Infrastructure.Configurations;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy("CORS",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

}

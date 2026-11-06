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
                    .AllowAnyHeader()
                    // Sin esto, el navegador repite el permiso previo casi en cada petición, y cuentan para el
                    // límite del hosting. Chrome y Android aplican hasta 2 horas; Safari, menos
                    .SetPreflightMaxAge(TimeSpan.FromHours(2)));
        });

}

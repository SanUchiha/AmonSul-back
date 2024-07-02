using Microsoft.Extensions.DependencyInjection;

namespace AS.Application.Dependencies
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsApplication(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("Cors",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        
    }
}

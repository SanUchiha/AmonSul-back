using AS.Application.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInjectionApplication(configuration);
        services.AddMapperApplication();
        services.AddInjectionEmailSender(configuration);

        return services;
    }
}
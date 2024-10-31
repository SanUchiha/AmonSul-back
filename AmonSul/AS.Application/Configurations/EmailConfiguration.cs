using AS.Application.DTOs.Email;
using AS.Application.Interfaces;
using AS.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Application.Configurations;

public static class EmailConfiguration
{
    public static IServiceCollection AddInjectionEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddTransient<IEmailApplicacion, EmailApplication>();

        return services;
    }

}

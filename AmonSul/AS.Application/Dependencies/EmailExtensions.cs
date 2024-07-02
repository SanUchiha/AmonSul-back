using AS.Application.Interfaces;
using AS.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Application.Dependencies
{
    public static class EmailExtensions
    {
        public static IServiceCollection AddInjectionEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEmailSender, EmailSender>();

            return services;
        }
    }
}

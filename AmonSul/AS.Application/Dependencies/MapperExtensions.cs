using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AS.Application.Dependencies
{
    public static class MapperExtensions
    {
        public static IServiceCollection AddMapperApplication(this IServiceCollection services) =>
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }
}

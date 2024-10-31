using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AS.Application.Configurations;

public static class MapperConfiguration
{
    public static IServiceCollection AddMapperApplication(this IServiceCollection services) =>
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace HorseRacing.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        // Add AutoMapper using Assembly scanning
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Features.UserManagement.Services;

namespace HorseRacing.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        // Add AutoMapper using Assembly scanning
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}

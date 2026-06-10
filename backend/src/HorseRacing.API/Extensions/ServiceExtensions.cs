using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Features.UserManagement.Services;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.BettingEngine.Services;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.Services;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Application.Features.Notifications.Services;

namespace HorseRacing.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        // Add AutoMapper using Assembly scanning
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IBettingService, BettingService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IBetPayoutService, BetPayoutService>();
        services.AddScoped<IPrizePayoutService, PrizePayoutService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}

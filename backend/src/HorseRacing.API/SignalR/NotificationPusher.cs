using System.Threading.Tasks;
using HorseRacing.Application.Common.Interfaces;
using HorseRacing.Application.Features.Notifications.DTOs;
using HorseRacing.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HorseRacing.API.SignalR;

public class NotificationPusher : INotificationPusher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationPusher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushToUserAsync(int userId, NotificationResponse notification)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
    }

    public async Task PushToAllAsync(NotificationResponse notification)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}

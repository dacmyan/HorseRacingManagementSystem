using System.Threading.Tasks;
using HorseRacing.Application.Features.Notifications.DTOs;

namespace HorseRacing.Application.Common.Interfaces;

public interface INotificationPusher
{
    Task PushToUserAsync(int userId, NotificationResponse notification);
    Task PushToAllAsync(NotificationResponse notification);
}

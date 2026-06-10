using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.Notifications.DTOs;

namespace HorseRacing.Application.Features.Notifications.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetNotificationsForUserAsync(int userId);
    Task MarkAsReadAsync(int id, int userId);
    Task SendNotificationAsync(SendNotificationRequest request);
}

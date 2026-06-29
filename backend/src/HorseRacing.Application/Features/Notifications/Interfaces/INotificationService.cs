using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.Notifications.DTOs;

namespace HorseRacing.Application.Features.Notifications.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetNotificationsForUserAsync(int userId);
    Task MarkAsReadAsync(int id, int userId);
    Task SendNotificationAsync(SendNotificationRequest request);
    Task<PagedNotificationResponse> GetNotificationsForUserPagedAsync(int userId, string? type, bool? isRead, int page, int pageSize);
    Task SendNotificationToUserAsync(int userId, string title, string content, string type, int? referenceId = null, string? thumbnail = null, string? actionUrl = null);
    Task BroadcastNotificationAsync(string title, string content, string type, int? referenceId = null, string? thumbnail = null, string? actionUrl = null);
    Task DeleteNotificationAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);
}

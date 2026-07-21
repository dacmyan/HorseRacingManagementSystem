using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Common.Interfaces;
using HorseRacing.Application.Features.Notifications.DTOs;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPusher _notificationPusher;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPusher notificationPusher)
    {
        _notificationRepository = notificationRepository;
        _notificationPusher = notificationPusher;
    }

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsForUserAsync(int userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        return notifications.Select(n => MapToResponse(n));
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        var notification = await _notificationRepository.GetByIdAndUserIdAsync(id, userId);

        if (notification == null)
        {
            throw new ArgumentException("Notification not found or access denied.");
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task SendNotificationAsync(SendNotificationRequest request)
    {
        await SendNotificationToUserAsync(
            request.UserId,
            "Notification",
            request.Message,
            "System"
        );
    }

    public async Task<PagedNotificationResponse> GetNotificationsForUserPagedAsync(int userId, string? type, bool? isRead, int page, int pageSize)
    {
        var (items, totalCount) = await _notificationRepository.GetPagedByUserIdAsync(userId, type, isRead, page, pageSize);
        return new PagedNotificationResponse
        {
            Items = items.Select(MapToResponse),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task SendNotificationToUserAsync(
        int userId, 
        string title, 
        string content, 
        string type, 
        int? referenceId = null, 
        string? thumbnail = null, 
        string? actionUrl = null)
    {
        var isUserActive = await _notificationRepository.IsUserActiveAsync(userId);
        if (!isUserActive)
        {
            // Do not send notification to inactive users
            return;
        }

        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            ReferenceId = referenceId,
            Thumbnail = thumbnail,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        // Push via SignalR
        await _notificationPusher.PushToUserAsync(userId, MapToResponse(notification));
    }

    public async Task SendNotificationToRoleAsync(
        string roleName, 
        string title, 
        string content, 
        string type, 
        int? referenceId = null, 
        string? thumbnail = null, 
        string? actionUrl = null)
    {
        var activeUserIds = await _notificationRepository.GetActiveUserIdsByRoleAsync(roleName);
        if (!activeUserIds.Any()) return;

        foreach (var userId in activeUserIds)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                ReferenceId = referenceId,
                Thumbnail = thumbnail,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _notificationRepository.AddAsync(notification);
        }
        await _notificationRepository.SaveChangesAsync();

        foreach (var userId in activeUserIds)
        {
            var notificationResponse = new NotificationResponse
            {
                UserId = userId,
                Title = title,
                Content = content,
                Message = content,
                Type = type,
                ReferenceId = referenceId,
                Thumbnail = thumbnail,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationPusher.PushToUserAsync(userId, notificationResponse);
        }
    }

    public async Task BroadcastNotificationAsync(
        string title, 
        string content, 
        string type, 
        int? referenceId = null, 
        string? thumbnail = null, 
        string? actionUrl = null)
    {
        var activeUserIds = await _notificationRepository.GetActiveUserIdsAsync();
        foreach (var userId in activeUserIds)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                ReferenceId = referenceId,
                Thumbnail = thumbnail,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _notificationRepository.AddAsync(notification);
        }
        await _notificationRepository.SaveChangesAsync();

        // Broadcast a generic real-time response so client context gets updated
        var genericResponse = new NotificationResponse
        {
            Id = 0,
            UserId = 0,
            Title = title,
            Content = content,
            Message = content,
            Type = type,
            ReferenceId = referenceId,
            Thumbnail = thumbnail,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationPusher.PushToAllAsync(genericResponse);
    }

    public async Task DeleteNotificationAsync(int id, int userId)
    {
        var notification = await _notificationRepository.GetByIdAndUserIdAsync(id, userId);
        if (notification == null)
        {
            throw new ArgumentException("Notification not found or access denied.");
        }
        notification.IsDeleted = true;
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        foreach (var n in notifications.Where(n => !n.IsRead))
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task<List<int>> GetActiveUserIdsByRoleAsync(string roleName)
    {
        return await _notificationRepository.GetActiveUserIdsByRoleAsync(roleName);
    }

    private static NotificationResponse MapToResponse(Notification n)
    {
        var content = string.IsNullOrEmpty(n.Content) ? (n.Message ?? string.Empty) : n.Content;
        var title = string.IsNullOrEmpty(n.Title) ? "Notification" : n.Title;
        var type = string.IsNullOrEmpty(n.Type) ? "System" : n.Type;

        return new NotificationResponse
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = title,
            Content = content,
            Message = content, // legacy support
            Type = type,
            ReferenceId = n.ReferenceId,
            Thumbnail = n.Thumbnail,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt,
            IsDeleted = n.IsDeleted
        };
    }
}

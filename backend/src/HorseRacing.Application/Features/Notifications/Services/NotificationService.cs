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
        ArgumentNullException.ThrowIfNull(request);
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
        ValidateNotification(userId, title, content, type, actionUrl);
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
        try
        {
            await _notificationPusher.PushToUserAsync(userId, MapToResponse(notification));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NOTIFICATION PUSH ERROR] User {userId}: {ex.Message}");
        }
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
        ValidateNotification(null, title, content, type, actionUrl);
        if (string.IsNullOrWhiteSpace(roleName) || roleName.Length > 50)
            throw new ArgumentException("Role name is required and cannot exceed 50 characters.", nameof(roleName));
        roleName = roleName.Trim();
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
            try
            {
                await _notificationPusher.PushToUserAsync(userId, notificationResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTIFICATION PUSH ERROR] User {userId}: {ex.Message}");
            }
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
        ValidateNotification(null, title, content, type, actionUrl);
        var activeUserIds = await _notificationRepository.GetActiveUserIdsAsync();
        var savedNotifications = new List<Notification>();
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
            savedNotifications.Add(notification);
        }
        await _notificationRepository.SaveChangesAsync();

        // Push only to active recipients, with each user's real persisted notification ID.
        foreach (var notification in savedNotifications)
        {
            try
            {
                await _notificationPusher.PushToUserAsync(notification.UserId, MapToResponse(notification));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTIFICATION PUSH ERROR] User {notification.UserId}: {ex.Message}");
            }
        }
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

    private static void ValidateNotification(int? userId, string title, string content, string type, string? actionUrl)
    {
        if (userId.HasValue && userId.Value <= 0)
            throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 150)
            throw new ArgumentException("Notification title is required and cannot exceed 150 characters.", nameof(title));
        if (string.IsNullOrWhiteSpace(content) || content.Trim().Length > 2000)
            throw new ArgumentException("Notification content is required and cannot exceed 2000 characters.", nameof(content));
        var allowedTypes = new[] { "Tournament", "Race", "Bet", "Wallet", "System", "Medical", "Contract", "Prediction", "Result", "Violation", "Registration", "Payout" };
        if (string.IsNullOrWhiteSpace(type) || !allowedTypes.Contains(type.Trim(), StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Notification type must be one of: {string.Join(", ", allowedTypes)}.", nameof(type));
        if (!string.IsNullOrWhiteSpace(actionUrl) &&
            (!actionUrl.StartsWith('/') || actionUrl.Length > 500 || actionUrl.StartsWith("//")))
            throw new ArgumentException("Action URL must be a local application path no longer than 500 characters.", nameof(actionUrl));
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

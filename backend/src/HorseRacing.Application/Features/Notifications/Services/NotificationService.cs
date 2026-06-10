using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.Notifications.DTOs;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsForUserAsync(int userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        return notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            UserId = n.UserId,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        var notification = await _notificationRepository.GetByIdAndUserIdAsync(id, userId);

        if (notification == null)
        {
            throw new ArgumentException("Notification not found or access denied.");
        }

        notification.IsRead = true;
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task SendNotificationAsync(SendNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message cannot be empty.");
        }

        var userExists = await _notificationRepository.UserExistsAsync(request.UserId);
        if (!userExists)
        {
            throw new ArgumentException($"User with ID {request.UserId} does not exist.");
        }

        var notification = new Notification
        {
            UserId = request.UserId,
            Message = request.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();
    }
}

using System.Collections.Generic;

namespace HorseRacing.Application.Features.Notifications.DTOs;

public class PagedNotificationResponse
{
    public IEnumerable<NotificationResponse> Items { get; set; } = new List<NotificationResponse>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

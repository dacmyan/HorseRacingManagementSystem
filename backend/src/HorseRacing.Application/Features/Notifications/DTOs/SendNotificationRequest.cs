namespace HorseRacing.Application.Features.Notifications.DTOs;

public class SendNotificationRequest
{
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}

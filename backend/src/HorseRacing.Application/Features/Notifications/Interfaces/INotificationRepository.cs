using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.Notifications.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<Notification?> GetByIdAndUserIdAsync(int id, int userId);
    Task<bool> UserExistsAsync(int userId);
    Task AddAsync(Notification notification);
    Task SaveChangesAsync();
}

using Inventario.Application.DTOs.Notifications;

namespace Inventario.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId);
    Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int userId);
    Task CreateLowStockNotificationAsync(int productId);
}

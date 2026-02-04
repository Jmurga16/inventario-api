using Inventario.Application.DTOs.Notifications;
using Inventario.Application.Interfaces;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;
using Inventario.Domain.Interfaces.Repositories;

namespace Inventario.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId)
    {
        var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
        return notifications.Select(MapToDto);
    }

    public async Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(int userId)
    {
        var notifications = await _unitOfWork.Notifications.GetUnreadByUserIdAsync(userId);
        return notifications.Select(MapToDto);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CreateLowStockNotificationAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null) return;

        var notificationType = product.Quantity == 0
            ? NotificationTypeEnum.OutOfStock
            : NotificationTypeEnum.LowStock;

        var title = notificationType == NotificationTypeEnum.OutOfStock
            ? $"Out of stock: {product.Name}"
            : $"Low stock: {product.Name}";

        var message = notificationType == NotificationTypeEnum.OutOfStock
            ? $"Product {product.SKU} - {product.Name} is out of stock."
            : $"Product {product.SKU} - {product.Name} has only {product.Quantity} units. Minimum required: {product.MinStock}";

        // Get admin users to notify
        var adminUserIds = await GetAdminUserIdsAsync();

        foreach (var userId in adminUserIds)
        {
            var notification = new Notification
            {
                UserId = userId,
                ProductId = productId,
                NotificationTypeId = (int)notificationType,
                Title = title,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<IEnumerable<int>> GetAdminUserIdsAsync()
    {
        return await _unitOfWork.Users.GetUserIdsByRoleAsync(RoleNames.Admin);
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            ProductId = notification.ProductId,
            NotificationTypeId = notification.NotificationTypeId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt
        };
    }
}

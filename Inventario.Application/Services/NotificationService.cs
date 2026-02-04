using Inventario.Application.DTOs.Notifications;
using Inventario.Application.Interfaces;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
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

        var notificationTypeId = product.Quantity == 0 ? 2 : 1; // OUT_OF_STOCK or LOW_STOCK
        var title = product.Quantity == 0
            ? $"Out of stock: {product.Name}"
            : $"Low stock: {product.Name}";
        var message = product.Quantity == 0
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
                NotificationTypeId = notificationTypeId,
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
        // TODO: Implement getting admin user IDs
        // For now, return empty list
        return await Task.FromResult(Array.Empty<int>());
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

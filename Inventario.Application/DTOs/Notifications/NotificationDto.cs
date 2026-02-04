namespace Inventario.Application.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int NotificationTypeId { get; set; }
    public string? NotificationTypeCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

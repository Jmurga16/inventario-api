using System.Security.Claims;
using Inventario.Application.DTOs.Common;
using Inventario.Application.DTOs.Notifications;
using Inventario.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetMyNotifications()
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notifications));
    }

    [HttpGet("unread")]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetUnread()
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUnreadByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notifications));
    }

    [HttpGet("unread/count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(ApiResponse.Ok("Notification marked as read"));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse.Ok("All notifications marked as read"));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim ?? "0");
    }
}

using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Interfaces;

public interface IOrderNotificationService
{
    // Notification management
    Task<OrderNotification?> CreateNotificationAsync(int orderId, int? adminId, NotificationType type, string message);
    Task<IEnumerable<OrderNotification>> GetUnreadNotificationsAsync(int adminId);
    Task<IEnumerable<OrderNotification>> GetAllNotificationsAsync(int adminId);
    Task<bool> MarkNotificationAsReadAsync(int notificationId, int adminId);
    Task<bool> MarkAllNotificationsAsReadAsync(int adminId);

    // Notification sending
    Task NotifyNewOrderAsync(int orderId);
    Task NotifyOrderReadyAsync(int orderId);
    Task NotifyOrderCancelledAsync(int orderId);

    // Notification broadcasting (SignalR)
    Task BroadcastOrderNotificationAsync(int orderId, NotificationType type);
}
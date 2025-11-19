using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using CafeManagement.Application.DTOs;

namespace CafeManagement.Infrastructure.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderNotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderNotification?> CreateNotificationAsync(int orderId, int? adminId, NotificationType type, string message)
    {
        try
        {
            var notification = new OrderNotification
            {
                OrderId = orderId,
                AdminId = adminId,
                Type = type,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.OrderNotifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return notification;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return null;
        }
    }

    public async Task<IEnumerable<OrderNotification>> GetUnreadNotificationsAsync(int adminId)
    {
        try
        {
            // Get notifications for specific admin or all admins (AdminId == null)
            var notifications = await _unitOfWork.OrderNotifications.FindAsync(
                n => (n.AdminId == adminId || n.AdminId == null) && !n.IsRead
            );

            return notifications.OrderByDescending(n => n.CreatedAt);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<OrderNotification>();
        }
    }

    public async Task<IEnumerable<OrderNotification>> GetAllNotificationsAsync(int adminId)
    {
        try
        {
            // Get notifications for specific admin or all admins (AdminId == null)
            var notifications = await _unitOfWork.OrderNotifications.FindAsync(
                n => n.AdminId == adminId || n.AdminId == null
            );

            return notifications.OrderByDescending(n => n.CreatedAt);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<OrderNotification>();
        }
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int adminId)
    {
        try
        {
            var notification = await _unitOfWork.OrderNotifications.GetByIdAsync(notificationId);
            if (notification == null || notification.IsRead)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.OrderNotifications.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return false;
        }
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync(int adminId)
    {
        try
        {
            var unreadNotifications = await _unitOfWork.OrderNotifications.FindAsync(
                n => (n.AdminId == adminId || n.AdminId == null) && !n.IsRead
            );

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.OrderNotifications.UpdateAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return false;
        }
    }

    public async Task NotifyNewOrderAsync(int orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return;

            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);
            var client = await _unitOfWork.Clients.GetByIdAsync(order.ClientId);

            var message = $"New order #{orderId} from {user?.Username ?? "Unknown"} at {client?.Name ?? "Unknown PC"} - Total: ${order.TotalAmount:F2}";

            // Create notification for all admins (AdminId == null)
            await CreateNotificationAsync(orderId, null, NotificationType.NewOrder, message);

            // Broadcast via SignalR (this will be implemented with SignalR hub)
            await BroadcastOrderNotificationAsync(orderId, NotificationType.NewOrder);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
        }
    }

    public async Task NotifyOrderReadyAsync(int orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return;

            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);

            var message = $"Order #{orderId} for {user?.Username ?? "Unknown"} is ready for pickup!";

            // Create notification for all admins (AdminId == null)
            await CreateNotificationAsync(orderId, null, NotificationType.OrderReady, message);

            // Broadcast via SignalR (this will be implemented with SignalR hub)
            await BroadcastOrderNotificationAsync(orderId, NotificationType.OrderReady);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
        }
    }

    public async Task NotifyOrderCancelledAsync(int orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return;

            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);

            var message = $"Order #{orderId} for {user?.Username ?? "Unknown"} has been cancelled";

            // Create notification for all admins (AdminId == null)
            await CreateNotificationAsync(orderId, null, NotificationType.OrderCancelled, message);

            // Broadcast via SignalR (this will be implemented with SignalR hub)
            await BroadcastOrderNotificationAsync(orderId, NotificationType.OrderCancelled);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
        }
    }

    public async Task BroadcastOrderNotificationAsync(int orderId, NotificationType type)
    {
        try
        {
            // This will be implemented when we add SignalR hub integration
            // For now, we'll just create the notification in the database

            // Get order details for broadcasting
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return;

            // Create notification message based on type
            var message = type switch
            {
                NotificationType.NewOrder => $"New order placed!",
                NotificationType.OrderReady => $"Order is ready for pickup",
                NotificationType.OrderCancelled => $"Order has been cancelled",
                _ => "Order notification"
            };

            // In the full implementation, this would send real-time notifications
            // through SignalR to all connected admin clients
            await Task.CompletedTask; // Placeholder for SignalR broadcasting
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
        }
    }
}
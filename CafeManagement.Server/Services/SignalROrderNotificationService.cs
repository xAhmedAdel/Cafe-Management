using CafeManagement.Application.DTOs;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using CafeManagement.Infrastructure.Data;
using CafeManagement.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProductDto = CafeManagement.Application.DTOs.ProductDto;
using OrderItemDto = CafeManagement.Application.DTOs.OrderItemDto;

namespace CafeManagement.Server.Services;

public class SignalROrderNotificationService : IOrderNotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<CafeManagementHub> _hubContext;
    private readonly ILogger<SignalROrderNotificationService> _logger;
    private readonly CafeManagementDbContext _dbContext;

    public SignalROrderNotificationService(
        IUnitOfWork unitOfWork,
        IHubContext<CafeManagementHub> hubContext,
        ILogger<SignalROrderNotificationService> logger,
        CafeManagementDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
        _dbContext = dbContext;
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
            _logger.LogError(ex, "Error creating order notification");
            return null;
        }
    }

    public async Task<IEnumerable<OrderNotification>> GetUnreadNotificationsAsync(int adminId)
    {
        try
        {
            return await _unitOfWork.OrderNotifications
                .FindAsync(n => n.AdminId == adminId && !n.IsRead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications");
            return Enumerable.Empty<OrderNotification>();
        }
    }

    public async Task<IEnumerable<OrderNotification>> GetAllNotificationsAsync(int adminId)
    {
        try
        {
            return await _unitOfWork.OrderNotifications
                .FindAsync(n => n.AdminId == adminId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all notifications");
            return Enumerable.Empty<OrderNotification>();
        }
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int adminId)
    {
        try
        {
            var notification = await _unitOfWork.OrderNotifications.GetByIdAsync(notificationId);
            if (notification == null || notification.AdminId != adminId)
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
            _logger.LogError(ex, "Error marking notification as read");
            return false;
        }
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync(int adminId)
    {
        try
        {
            var notifications = await _unitOfWork.OrderNotifications
                .FindAsync(n => n.AdminId == adminId && !n.IsRead);

            foreach (var notification in notifications)
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
            _logger.LogError(ex, "Error marking all notifications as read");
            return false;
        }
    }

    public async Task NotifyNewOrderAsync(int orderId)
    {
        _logger.LogInformation($"🛒 Processing new order notification for Order #{orderId}");

        // Create notification in database
        await CreateNotificationAsync(orderId, null, NotificationType.NewOrder,
            $"New order #{orderId} has been placed");

        // Broadcast via SignalR is handled by BroadcastOrderNotificationAsync
    }

    public async Task NotifyOrderReadyAsync(int orderId)
    {
        _logger.LogInformation($"🔔 Processing order ready notification for Order #{orderId}");

        // Create notification in database
        await CreateNotificationAsync(orderId, null, NotificationType.OrderReady,
            $"Order #{orderId} is ready for pickup");

        // Broadcast via SignalR is handled by BroadcastOrderNotificationAsync
    }

    public async Task NotifyOrderCancelledAsync(int orderId)
    {
        _logger.LogInformation($"❌ Processing order cancelled notification for Order #{orderId}");

        // Create notification in database
        await CreateNotificationAsync(orderId, null, NotificationType.OrderCancelled,
            $"Order #{orderId} has been cancelled");

        // Broadcast via SignalR is handled by BroadcastOrderNotificationAsync
    }

    public async Task BroadcastOrderNotificationAsync(int orderId, NotificationType type)
    {
        try
        {
            _logger.LogInformation($"📡 Broadcasting SignalR notification for Order #{orderId}, Type: {type}");

            // Get the complete order with navigation properties
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning($"Order #{orderId} not found for broadcasting");
                return;
            }

            // Map to OrderDto
            var orderDto = MapToOrderDto(order);

            // Broadcast based on notification type
            switch (type)
            {
                case NotificationType.NewOrder:
                    await _hubContext.Clients.Group("Operators").SendAsync("NewOrderReceived", orderDto);
                    await _hubContext.Clients.Group("Administrators").SendAsync("NewOrderReceived", orderDto);
                    _logger.LogInformation($"✅ New order #{orderId} broadcasted to operators and administrators");
                    break;

                case NotificationType.OrderReady:
                    await _hubContext.Clients.Group("Operators").SendAsync("OrderReadyForPickup", orderDto);
                    await _hubContext.Clients.Group("Administrators").SendAsync("OrderReadyForPickup", orderDto);

                    // Also notify the client who placed the order
                    if (order.ClientId > 0)
                    {
                        await _hubContext.Clients.Group($"Client_{order.ClientId}").SendAsync("OrderReadyForPickup", orderDto);
                    }
                    _logger.LogInformation($"✅ Order ready #{orderId} broadcasted");
                    break;

                case NotificationType.OrderCancelled:
                    await _hubContext.Clients.Group("Operators").SendAsync("OrderCancelled", orderDto);
                    await _hubContext.Clients.Group("Administrators").SendAsync("OrderCancelled", orderDto);

                    // Also notify the client who placed the order
                    if (order.ClientId > 0)
                    {
                        await _hubContext.Clients.Group($"Client_{order.ClientId}").SendAsync("OrderCancelled", orderDto);
                    }
                    _logger.LogInformation($"✅ Order cancelled #{orderId} broadcasted");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting SignalR notification for Order #{orderId}");
        }
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            ClientId = order.ClientId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CustomerNotes = order.CustomerNotes,
            CreatedAt = order.CreatedAt,
            CompletedAt = order.CompletedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Product = new ProductDto
                {
                    Id = oi.Product.Id,
                    Name = oi.Product.Name,
                    Price = oi.Product.Price,
                    Category = oi.Product.Category,
                    ImageUrl = oi.Product.ImageUrl,
                    IsAvailable = oi.Product.IsAvailable,
                    DisplayOrder = oi.Product.DisplayOrder,
                    PreparationTimeMinutes = oi.Product.PreparationTimeMinutes,
                    CreatedAt = oi.Product.CreatedAt,
                    UpdatedAt = oi.Product.UpdatedAt
                }
            }).ToList()
        };
    }
}
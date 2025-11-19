using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CafeManagement.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly IOrderNotificationService _notificationService;

    public OrderService(
        IUnitOfWork unitOfWork,
        IProductService productService,
        IOrderNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _productService = productService;
        _notificationService = notificationService;
    }

    public async Task<Order?> CreateOrderAsync(int userId, int clientId, CreateOrderRequest request)
    {
        try
        {
            // Validate the order
            if (!await ValidateOrderAsync(request))
                return null;

            // Check if user can afford the order
            var totalAmount = await CalculateOrderTotalAsync(request);
            if (!await CanUserAffordOrderAsync(userId, totalAmount))
                return null;

            var order = new Order
            {
                UserId = userId,
                ClientId = clientId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                CustomerNotes = request.CustomerNotes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add order items
            var orderItems = new List<OrderItem>();
            foreach (var itemRequest in request.Items)
            {
                var product = await _productService.GetProductByIdAsync(itemRequest.ProductId);
                if (product == null || !product.IsAvailable)
                    return null;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id, // Will be set after order is saved
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                orderItems.Add(orderItem);
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Now set the OrderId for each order item
            foreach (var orderItem in orderItems)
            {
                orderItem.OrderId = order.Id;
                await _unitOfWork.OrderItems.AddAsync(orderItem);
            }

            await _unitOfWork.SaveChangesAsync();

            // Load the complete order with navigation properties
            var completeOrder = await GetOrderByIdAsync(order.Id);

            // Send SignalR notification (broadcast to all connected operators and administrators)
            await _notificationService.BroadcastOrderNotificationAsync(order.Id, NotificationType.NewOrder);

            // Send notification to admins
            await _notificationService.NotifyNewOrderAsync(order.Id);

            return completeOrder;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return null;
        }
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return null;

            // Load order items
            var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == orderId);
            order.OrderItems = orderItems.ToList();

            // Load navigation properties if needed
            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);
            if (user != null)
                order.User = user;

            var client = await _unitOfWork.Clients.GetByIdAsync(order.ClientId);
            if (client != null)
                order.Client = client;

            return order;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return null;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
    {
        try
        {
            var orders = await _unitOfWork.Orders.FindAsync(o => o.UserId == userId);
            return orders.OrderByDescending(o => o.CreatedAt);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByClientAsync(int clientId)
    {
        try
        {
            var orders = await _unitOfWork.Orders.FindAsync(o => o.ClientId == clientId);
            return orders.OrderByDescending(o => o.CreatedAt);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return orders.OrderByDescending(o => o.CreatedAt);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        try
        {
            var orders = await _unitOfWork.Orders.FindAsync(o => o.Status == OrderStatus.Pending);
            return orders.OrderBy(o => o.CreatedAt);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        try
        {
            var orders = await _unitOfWork.Orders.FindAsync(o => o.Status == status);
            return status == OrderStatus.Pending ? orders.OrderBy(o => o.CreatedAt) : orders.OrderByDescending(o => o.CreatedAt);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, string? adminNotes = null)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return false;

            var oldStatus = order.Status;
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Send notifications based on status change
            if (oldStatus != OrderStatus.Ready && status == OrderStatus.Ready)
            {
                await _notificationService.NotifyOrderReadyAsync(orderId);
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return false;
        }
    }

    public async Task<bool> ConfirmOrderAsync(int orderId, int adminId)
    {
        return await UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed);
    }

    public async Task<bool> MarkOrderReadyAsync(int orderId, int adminId)
    {
        return await UpdateOrderStatusAsync(orderId, OrderStatus.Ready);
    }

    public async Task<bool> CompleteOrderAsync(int orderId, int adminId)
    {
        return await UpdateOrderStatusAsync(orderId, OrderStatus.Completed);
    }

    public async Task<bool> CancelOrderAsync(int orderId, string reason)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || !order.CanBeCancelled)
                return false;

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.NotifyOrderCancelledAsync(orderId);

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
        await _notificationService.NotifyNewOrderAsync(orderId);
    }

    public async Task NotifyOrderReadyAsync(int orderId)
    {
        await _notificationService.NotifyOrderReadyAsync(orderId);
    }

    public async Task NotifyOrderCancelledAsync(int orderId)
    {
        await _notificationService.NotifyOrderCancelledAsync(orderId);
    }

    public async Task<bool> ValidateOrderAsync(CreateOrderRequest request)
    {
        if (request == null || !request.Items.Any())
            return false;

        // Check if all products exist and are available
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        if (!await AreProductsAvailableAsync(productIds))
            return false;

        // Check quantities
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return false;
        }

        return true;
    }

    public async Task<decimal> CalculateOrderTotalAsync(CreateOrderRequest request)
    {
        decimal total = 0;

        foreach (var item in request.Items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product != null)
            {
                total += product.Price * item.Quantity;
            }
        }

        return total;
    }

    public async Task<bool> CanUserAffordOrderAsync(int userId, decimal totalAmount)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            return user.Balance >= totalAmount;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return false;
        }
    }

    public async Task<bool> AreProductsAvailableAsync(List<int> productIds)
    {
        foreach (var productId in productIds)
        {
            if (!await _productService.IsProductAvailableAsync(productId))
                return false;
        }
        return true;
    }

    public async Task<OrderStatsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            IEnumerable<Order> orders = await _unitOfWork.Orders.GetAllAsync();

            if (startDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= endDate.Value);

            var ordersList = orders.ToList();

            return new OrderStatsDto
            {
                TotalOrders = ordersList.Count,
                PendingOrders = ordersList.Count(o => o.Status == OrderStatus.Pending),
                ReadyOrders = ordersList.Count(o => o.Status == OrderStatus.Ready),
                CompletedOrders = ordersList.Count(o => o.Status == OrderStatus.Completed),
                TotalRevenue = ordersList.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
                TodayRevenue = ordersList.Where(o => o.Status == OrderStatus.Completed && o.CreatedAt.Date == DateTime.Today)
                                      .Sum(o => o.TotalAmount),
                AverageOrderValue = ordersList.Any() ? (double)ordersList.Average(o => o.TotalAmount) : 0,
                PopularProducts = await GetPopularProductsAsync()
            };
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return new OrderStatsDto();
        }
    }

    public async Task<List<Product>> GetPopularProductsAsync(int topCount = 10, DateTime? startDate = null)
    {
        try
        {
            IEnumerable<OrderItem> orderItems = await _unitOfWork.OrderItems.GetAllAsync();

            if (startDate.HasValue)
                orderItems = orderItems.Where(oi => oi.CreatedAt >= startDate.Value);

            var productCounts = orderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToList();

            var popularProducts = new List<Product>();
            foreach (var item in productCounts)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                    popularProducts.Add(product);
            }

            return popularProducts;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return new List<Product>();
        }
    }
}
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Interfaces;

// Request DTOs moved to Core layer
public class CreateOrderRequest
{
    public List<CreateOrderItemRequest> Items { get; set; } = new();
    public string? CustomerNotes { get; set; }
}

public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ReadyOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public double AverageOrderValue { get; set; }
    public List<Product> PopularProducts { get; set; } = new();
}

public interface IOrderService
{
    // Order management
    Task<Order?> CreateOrderAsync(int userId, int clientId, CreateOrderRequest request);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
    Task<IEnumerable<Order>> GetOrdersByClientAsync(int clientId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<IEnumerable<Order>> GetPendingOrdersAsync();
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

    // Order status updates
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, string? adminNotes = null);
    Task<bool> ConfirmOrderAsync(int orderId, int adminId);
    Task<bool> MarkOrderReadyAsync(int orderId, int adminId);
    Task<bool> CompleteOrderAsync(int orderId, int adminId);
    Task<bool> CancelOrderAsync(int orderId, string reason);

    // Order notifications
    Task NotifyNewOrderAsync(int orderId);
    Task NotifyOrderReadyAsync(int orderId);
    Task NotifyOrderCancelledAsync(int orderId);

    // Order validation and business logic
    Task<bool> ValidateOrderAsync(CreateOrderRequest request);
    Task<decimal> CalculateOrderTotalAsync(CreateOrderRequest request);
    Task<bool> CanUserAffordOrderAsync(int userId, decimal totalAmount);
    Task<bool> AreProductsAvailableAsync(List<int> productIds);

    // Order statistics
    Task<OrderStatsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<Product>> GetPopularProductsAsync(int topCount = 10, DateTime? startDate = null);
}
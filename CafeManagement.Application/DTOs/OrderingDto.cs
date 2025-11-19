using CafeManagement.Core.Enums;

namespace CafeManagement.Application.DTOs;

// Product DTOs
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int DisplayOrder { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Computed properties
    public string CategoryName => Category.ToString();
    public string FormattedPrice => $"{Price:F2} L.E";

    // UI binding property (not persisted)
    public int Quantity { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public int PreparationTimeMinutes { get; set; } = 5;
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public ProductCategory? Category { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsAvailable { get; set; }
    public int? DisplayOrder { get; set; }
    public int? PreparationTimeMinutes { get; set; }
}

// Order DTOs
public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ClientId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? CustomerNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation data
    public UserDto? User { get; set; }
    public ClientDto? Client { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();

    // Computed properties
    public string StatusText => Status.ToString();
    public string FormattedTotal => $"{TotalAmount:F2} L.E";
    public int TotalItems => OrderItems?.Sum(item => item.Quantity) ?? 0;
    public bool IsPending => Status == OrderStatus.Pending;
    public bool IsReady => Status == OrderStatus.Ready;
    public bool IsCompleted => Status == OrderStatus.Completed;
    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;

    // Navigation data
    public ProductDto? Product { get; set; }

    // Computed properties
    public string FormattedUnitPrice => $"{UnitPrice:F2} L.E";
    public string FormattedSubtotal => $"{Subtotal:F2} L.E";
    public string ProductName => Product?.Name ?? "Unknown Product";
}

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

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}

// Order Notification DTOs
public class OrderNotificationDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? AdminId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation data
    public OrderDto? Order { get; set; }
    public UserDto? Admin { get; set; }

    // Computed properties
    public string TypeText => Type.ToString();
    public bool IsNewOrder => Type == NotificationType.NewOrder;
    public bool IsOrderReady => Type == NotificationType.OrderReady;
    public bool IsUnread => !IsRead;
}

// Menu Response DTOs
public class MenuCategoryDto
{
    public ProductCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new();
    public int AvailableProductCount => Products.Count(p => p.IsAvailable);
}

public class MenuResponse
{
    public List<MenuCategoryDto> Categories { get; set; } = new();
    public int TotalProducts { get; set; }
    public int AvailableProducts { get; set; }
}

// Order Statistics DTOs
public class OrderStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ReadyOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public double AverageOrderValue { get; set; }
    public List<CafeManagement.Core.Entities.Product> PopularProducts { get; set; } = new();
}

// Cart DTOs (for client-side cart management)
public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
    public string ImageUrl { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public string FormattedUnitPrice => $"{UnitPrice:F2} L.E";
    public string FormattedSubtotal => $"{Subtotal:F2} L.E";
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(item => item.Subtotal);
    public int TotalItems => Items.Sum(item => item.Quantity);
    public string FormattedTotal => $"{TotalAmount:F2} L.E";
}
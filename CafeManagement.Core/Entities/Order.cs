using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Entities;

public class Order : BaseEntity
{
    public int UserId { get; set; }
    public int ClientId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? CustomerNotes { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Client Client { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<OrderNotification> Notifications { get; set; } = new List<OrderNotification>();

    // Computed properties
    public string StatusText => Status.ToString();
    public string FormattedTotal => $"${TotalAmount:F2}";
    public int TotalItems => OrderItems?.Sum(item => item.Quantity) ?? 0;
    public bool IsPending => Status == OrderStatus.Pending;
    public bool IsReady => Status == OrderStatus.Ready;
    public bool IsCompleted => Status == OrderStatus.Completed;
    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
}
using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Entities;

public class OrderNotification : BaseEntity
{
    public int OrderId { get; set; }
    public int? AdminId { get; set; } // null means sent to all admins
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public User? Admin { get; set; }

    // Computed properties
    public string TypeText => Type.ToString();
    public bool IsNewOrder => Type == NotificationType.NewOrder;
    public bool IsOrderReady => Type == NotificationType.OrderReady;
    public bool IsUnread => !IsRead;
}
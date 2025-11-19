namespace CafeManagement.Core.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;

    // Computed properties
    public string FormattedUnitPrice => $"${UnitPrice:F2}";
    public string FormattedSubtotal => $"${Subtotal:F2}";
    public string ProductName => Product?.Name ?? "Unknown Product";
}
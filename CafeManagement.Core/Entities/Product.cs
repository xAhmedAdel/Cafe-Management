using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public int PreparationTimeMinutes { get; set; } = 5; // Default preparation time

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Computed properties
    public string CategoryName => Category.ToString();
    public string FormattedPrice => $"${Price:F2}";
}
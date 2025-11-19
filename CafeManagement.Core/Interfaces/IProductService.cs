using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Interfaces;

// Request DTOs (these could be moved to Core layer or create separate request models)
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

public class MenuResponse
{
    public List<MenuCategoryDto> Categories { get; set; } = new();
    public int TotalProducts { get; set; }
    public int AvailableProducts { get; set; }
}

public class MenuCategoryDto
{
    public ProductCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new();
    public int AvailableProductCount => Products.Count(p => p.IsAvailable);
}

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
    public string FormattedPrice => $"${Price:F2}";
}

public interface IProductService
{
    // Product management
    Task<Product?> CreateProductAsync(CreateProductRequest request);
    Task<Product?> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int productId);
    Task<Product?> GetProductByIdAsync(int productId);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(ProductCategory category);
    Task<IEnumerable<Product>> GetAvailableProductsAsync();
    Task<MenuResponse> GetMenuAsync();

    // Product availability
    Task<bool> SetProductAvailabilityAsync(int productId, bool isAvailable);
    Task<bool> IsProductAvailableAsync(int productId);
}
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;

namespace CafeManagement.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Product?> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
                ImageUrl = request.ImageUrl,
                IsAvailable = request.IsAvailable,
                DisplayOrder = request.DisplayOrder,
                PreparationTimeMinutes = request.PreparationTimeMinutes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return null;
        }
    }

    public async Task<Product?> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return null;

            // Update only provided fields
            if (request.Name != null) product.Name = request.Name;
            if (request.Description != null) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.Category.HasValue) product.Category = request.Category.Value;
            if (request.ImageUrl != null) product.ImageUrl = request.ImageUrl;
            if (request.IsAvailable.HasValue) product.IsAvailable = request.IsAvailable.Value;
            if (request.DisplayOrder.HasValue) product.DisplayOrder = request.DisplayOrder.Value;
            if (request.PreparationTimeMinutes.HasValue) product.PreparationTimeMinutes = request.PreparationTimeMinutes.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return null;
        }
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return false;

            // Check if product is in any active orders
            var activeOrders = await _unitOfWork.Orders.FindAsync(o => o.OrderItems.Any(oi => oi.ProductId == productId && o.Status != OrderStatus.Completed));
            if (activeOrders.Any())
            {
                // Instead of deleting, mark as unavailable
                product.IsAvailable = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Products.UpdateAsync(product);
            }
            else
            {
                await _unitOfWork.Products.DeleteAsync(product);
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

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        try
        {
            return await _unitOfWork.Products.GetByIdAsync(productId);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return null;
        }
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        try
        {
            return await _unitOfWork.Products.GetAllAsync();
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Product>();
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(ProductCategory category)
    {
        try
        {
            return await _unitOfWork.Products.FindAsync(p => p.Category == category && p.IsAvailable);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Product>();
        }
    }

    public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
    {
        try
        {
            return await _unitOfWork.Products.FindAsync(p => p.IsAvailable);
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return Enumerable.Empty<Product>();
        }
    }

    public async Task<MenuResponse> GetMenuAsync()
    {
        try
        {
            var allProducts = await _unitOfWork.Products.FindAsync(p => p.IsAvailable);
            var groupedProducts = allProducts.GroupBy(p => p.Category);

            var categories = groupedProducts.Select(g => new MenuCategoryDto
            {
                Category = g.Key,
                CategoryName = g.Key.ToString(),
                Products = g.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
                                .Select(p => MapToProductDto(p)).ToList()
            }).OrderBy(c => c.Category).ToList();

            return new MenuResponse
            {
                Categories = categories,
                TotalProducts = allProducts.Count(),
                AvailableProducts = allProducts.Count(p => p.IsAvailable)
            };
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return new MenuResponse();
        }
    }

    public async Task<bool> SetProductAvailabilityAsync(int productId, bool isAvailable)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return false;

            product.IsAvailable = isAvailable;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return false;
        }
    }

    public async Task<bool> IsProductAvailableAsync(int productId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            return product?.IsAvailable ?? false;
        }
        catch (Exception ex)
        {
            // Log error here when logging system is implemented
            return false;
        }
    }

    private ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            IsAvailable = product.IsAvailable,
            DisplayOrder = product.DisplayOrder,
            PreparationTimeMinutes = product.PreparationTimeMinutes,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
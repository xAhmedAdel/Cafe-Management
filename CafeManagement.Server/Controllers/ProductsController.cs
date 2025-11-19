using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CafeManagement.Core.Interfaces;
using CafeManagement.Application.DTOs;

// Use aliases to avoid ambiguity
using ProductDto = CafeManagement.Core.Interfaces.ProductDto;
using CreateProductRequest = CafeManagement.Core.Interfaces.CreateProductRequest;
using UpdateProductRequest = CafeManagement.Core.Interfaces.UpdateProductRequest;
using MenuResponse = CafeManagement.Core.Interfaces.MenuResponse;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for product management
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var productDtos = products.Select(MapToProductDto);
            return Ok(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get available products (for customer ordering)
    /// </summary>
    [HttpGet("available")]
    [AllowAnonymous] // Allow anonymous access for customer menu
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAvailable()
    {
        try
        {
            var products = await _productService.GetAvailableProductsAsync();
            var productDtos = products.Select(MapToProductDto);
            return Ok(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    [AllowAnonymous] // Allow anonymous access for customer menu
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(CafeManagement.Core.Enums.ProductCategory category)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(category);
            var productDtos = products.Select(MapToProductDto);
            return Ok(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {Category}", category);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all products for admin panel (no authentication required)
    /// </summary>
    [HttpGet("admin")]
    [AllowAnonymous] // Allow anonymous access for admin panel
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAdminProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var productDtos = products.Select(MapToProductDto);
            return Ok(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get full menu organized by categories
    /// </summary>
    [HttpGet("menu")]
    [AllowAnonymous] // Allow anonymous access for customer menu
    public async Task<ActionResult<MenuResponse>> GetMenu()
    {
        try
        {
            var menu = await _productService.GetMenuAsync();
            return Ok(menu);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only admins can create products
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.CreateProductAsync(request);
            if (product == null)
                return BadRequest("Failed to create product");

            _logger.LogInformation("Product {ProductName} created by user", product.Name);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Only admins can update products
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateProductAsync(id, request);
            if (product == null)
                return NotFound();

            _logger.LogInformation("Product {ProductId} updated by user", id);
            return Ok(MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only admins can delete products
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound();

            _logger.LogInformation("Product {ProductId} deleted by user", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Toggle product availability
    /// </summary>
    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "Admin")] // Only admins can change availability
    public async Task<ActionResult> ToggleAvailability(int id, [FromBody] bool isAvailable)
    {
        try
        {
            var result = await _productService.SetProductAvailabilityAsync(id, isAvailable);
            if (!result)
                return NotFound();

            _logger.LogInformation("Product {ProductId} availability set to {IsAvailable} by user", id, isAvailable);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product availability {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private ProductDto MapToProductDto(CafeManagement.Core.Entities.Product product)
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
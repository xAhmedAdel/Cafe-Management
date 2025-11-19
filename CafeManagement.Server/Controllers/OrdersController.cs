using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CafeManagement.Core.Interfaces;
using CafeManagement.Core.Enums;
using CafeManagement.Application.DTOs;

// Use aliases to avoid ambiguity
using CreateOrderRequest = CafeManagement.Core.Interfaces.CreateOrderRequest;
using OrderStatsDto = CafeManagement.Core.Interfaces.OrderStatsDto;
using OrderDto = CafeManagement.Application.DTOs.OrderDto;
using OrderItemDto = CafeManagement.Application.DTOs.OrderItemDto;
using ProductDto = CafeManagement.Application.DTOs.ProductDto;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderNotificationService _notificationService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        IOrderNotificationService notificationService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [Authorize] // Users must be logged in to place orders
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get user ID from JWT token (you'll need to implement this)
            var userId = GetCurrentUserId();
            var clientId = GetCurrentClientId();

            if (userId == null || clientId == null)
                return Unauthorized();

            var order = await _orderService.CreateOrderAsync(userId.Value, clientId.Value, request);
            if (order == null)
                return BadRequest("Failed to create order. Please check your balance and product availability.");

            _logger.LogInformation("Order {OrderId} created by user {UserId}", order.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToOrderDto(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            // Check if user has permission to view this order
            if (!CanUserViewOrder(order))
                return Forbid();

            return Ok(MapToOrderDto(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orders for current user
    /// </summary>
    [HttpGet("my-orders")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var orders = await _orderService.GetOrdersByUserAsync(userId.Value);
            var orderDtos = orders.Select(MapToOrderDto);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all orders (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var orderDtos = orders.Select(MapToOrderDto);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get pending orders (admin only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetPending()
    {
        try
        {
            var orders = await _orderService.GetPendingOrdersAsync();
            var orderDtos = orders.Select(MapToOrderDto);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orders by status (admin only)
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStatus(OrderStatus status)
    {
        try
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            var orderDtos = orders.Select(MapToOrderDto);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by status {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update order status (admin only)
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status, request.AdminNotes);
            if (!result)
                return NotFound();

            var adminId = GetCurrentUserId();
            _logger.LogInformation("Order {OrderId} status updated to {Status} by admin {AdminId}", id, request.Status, adminId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Confirm order (admin only)
    /// </summary>
    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult> ConfirmOrder(int id)
    {
        try
        {
            var result = await _orderService.ConfirmOrderAsync(id, GetCurrentUserId() ?? 0);
            if (!result)
                return NotFound();

            _logger.LogInformation("Order {OrderId} confirmed by admin", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark order as ready (admin only)
    /// </summary>
    [HttpPost("{id}/ready")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult> MarkAsReady(int id)
    {
        try
        {
            var result = await _orderService.MarkOrderReadyAsync(id, GetCurrentUserId() ?? 0);
            if (!result)
                return NotFound();

            _logger.LogInformation("Order {OrderId} marked as ready by admin", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as ready", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Complete order (admin only)
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult> CompleteOrder(int id)
    {
        try
        {
            var result = await _orderService.CompleteOrderAsync(id, GetCurrentUserId() ?? 0);
            if (!result)
                return NotFound();

            _logger.LogInformation("Order {OrderId} completed by admin", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult> CancelOrder(int id, [FromBody] string reason)
    {
        try
        {
            // Get the order to check permissions
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            // Users can only cancel their own pending orders
            var userId = GetCurrentUserId();
            var userRole = GetUserRole();

            if (userRole != "Admin" && order.UserId != userId)
                return Forbid();

            if (!order.CanBeCancelled)
                return BadRequest("This order cannot be cancelled");

            var result = await _orderService.CancelOrderAsync(id, reason);
            if (!result)
                return BadRequest("Failed to cancel order");

            _logger.LogInformation("Order {OrderId} cancelled by user {UserId} with reason: {Reason}", id, userId, reason);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get order statistics (admin only)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<ActionResult<OrderStatsDto>> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _orderService.GetOrderStatisticsAsync(startDate, endDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    // Helper methods
    private int? GetCurrentUserId()
    {
        // TODO: Implement JWT token parsing to get user ID
        // This is a placeholder - you'll need to implement proper JWT token parsing
        return User.FindFirst("UserId")?.Value != null ? int.Parse(User.FindFirst("UserId")!.Value) : null;
    }

    private int? GetCurrentClientId()
    {
        // TODO: Implement client ID extraction from context or token
        // This is a placeholder
        return 1; // Default to client ID 1 for now
    }

    private string? GetUserRole()
    {
        // TODO: Implement role extraction from JWT token
        return User.FindFirst("Role")?.Value;
    }

    private bool CanUserViewOrder(CafeManagement.Core.Entities.Order order)
    {
        var userRole = GetUserRole();
        var userId = GetCurrentUserId();

        // Admins and operators can view all orders
        if (userRole == "Admin" || userRole == "Operator")
            return true;

        // Users can only view their own orders
        return order.UserId == userId;
    }

    private OrderDto MapToOrderDto(CafeManagement.Core.Entities.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            ClientId = order.ClientId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CustomerNotes = order.CustomerNotes,
            CreatedAt = order.CreatedAt,
            CompletedAt = order.CompletedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Product = order.OrderItems.FirstOrDefault(i => i.ProductId == oi.ProductId)?.Product != null
                    ? new CafeManagement.Application.DTOs.ProductDto
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        Price = oi.Product.Price
                    }
                    : null
            }).ToList()
        };
    }
}
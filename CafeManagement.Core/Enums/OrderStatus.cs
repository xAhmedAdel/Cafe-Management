namespace CafeManagement.Core.Enums;

public enum OrderStatus
{
    Pending = 0,      // Order placed, waiting for admin confirmation
    Confirmed = 1,    // Admin confirmed, preparing
    Ready = 2,        // Order ready for pickup/delivery
    Completed = 3,    // Order completed and charged
    Cancelled = 4     // Order cancelled
}

public enum NotificationType
{
    NewOrder = 0,     // New order placed by customer
    OrderReady = 1,   // Order is ready for pickup
    OrderCancelled = 2 // Order was cancelled
}

public enum ProductCategory
{
    Drinks = 0,       // Hot and cold drinks
    Food = 1,         // Meals and food items
    Snacks = 2        // Snacks and small items
}
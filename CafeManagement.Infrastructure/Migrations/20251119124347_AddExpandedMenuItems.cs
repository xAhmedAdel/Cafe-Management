using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpandedMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // More Drinks Category (Category 0) - Additional items after the existing 5
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsAvailable", "Name", "PreparationTimeMinutes", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    // Coffee Variations
                    { 14, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stronger espresso shot", 6, "", true, "Double Espresso", 3, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Espresso with hot water", 7, "", true, "Americano", 3, 3.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Foamy steamed milk", 8, "", true, "Macchiato", 4, 4.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chocolate and coffee", 9, "", true, "Mocha", 5, 5.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vanilla, caramel, or hazelnut", 10, "", true, "Flavored Latte", 5, 5.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cold brewed coffee", 11, "", true, "Iced Coffee", 3, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Coffee with ice cream", 12, "", true, "Affogato", 3, 4.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // Tea Varieties
                    { 21, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Earl Grey with bergamot", 13, "", true, "Earl Grey Tea", 3, 2.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Traditional green tea", 14, "", true, "Green Tea", 3, 2.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Refreshing mint tea", 15, "", true, "Peppermint Tea", 3, 2.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chamomile calming blend", 16, "", true, "Chamomile Tea", 3, 2.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Spiced Indian tea with milk", 17, "", true, "Chai Latte", 5, 4.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Creamy bubble tea", 18, "", true, "Bubble Tea", 8, 6.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sweet milk tea with boba", 19, "", true, "Thai Iced Tea", 5, 4.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // Cold Drinks
                    { 28, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lemonade with mint", 20, "", true, "Fresh Lemonade", 3, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fresh fruit smoothie", 21, "", true, "Fruit Smoothie", 4, 5.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Frozen mango drink", 22, "", true, "Mango Lassi", 3, 4.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chocolate milkshake", 23, "", true, "Chocolate Shake", 4, 5.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vanilla milkshake", 24, "", true, "Vanilla Shake", 4, 5.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 33, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mixed berry shake", 25, "", true, "Strawberry Shake", 4, 5.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ice cold water", 26, "", true, "Mineral Water", 1, 1.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carbonated soft drink", 27, "", true, "Soda Pop", 1, 2.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            // Food Category (Category 1) - Additional items after the existing 4
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsAvailable", "Name", "PreparationTimeMinutes", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    // Breakfast Items
                    { 36, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Two eggs cooked to order", 5, "", true, "Scrambled Eggs", 5, 4.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 37, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fluffy golden pancakes", 6, "", true, "Pancakes", 8, 6.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 38, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Belgian waffles with syrup", 7, "", true, "Waffles", 8, 7.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 39, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "French toast with cinnamon", 8, "", true, "French Toast", 8, 6.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 40, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Three strips crispy bacon", 9, "", true, "Bacon Strips", 6, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Breakfast sausage links", 10, "", true, "Sausage Links", 6, 4.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 42, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hash brown potatoes", 11, "", true, "Hash Browns", 5, 3.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 43, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Oatmeal with toppings", 12, "", true, "Oatmeal", 5, 4.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 44, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Eggs Benedict", 13, "", true, "Eggs Benedict", 10, 9.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 45, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yogurt with granola and berries", 14, "", true, "Yogurt Parfait", 3, 5.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // More Sandwiches & Wraps
                    { 46, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grilled cheese sandwich", 15, "", true, "Grilled Cheese", 6, 4.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 47, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken club sandwich", 16, "", true, "Club Sandwich", 8, 7.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 48, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Turkey and avocado wrap", 17, "", true, "Turkey Wrap", 8, 7.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 49, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vegetarian wrap", 18, "", true, "Veggie Wrap", 8, 6.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 50, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tuna melt sandwich", 19, "", true, "Tuna Melt", 7, 6.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 51, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Panini pressed sandwich", 20, "", true, "Italian Panini", 8, 7.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 52, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reuben sandwich", 21, "", true, "Reuben", 8, 8.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // More Main Courses
                    { 53, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cheeseburger deluxe", 22, "", true, "Cheeseburger", 12, 9.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 54, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Double patty burger", 23, "", true, "Double Burger", 15, 11.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 55, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Veggie burger", 24, "", true, "Veggie Burger", 10, 7.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 56, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken sandwich", 25, "", true, "Chicken Burger", 12, 8.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 57, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fish and chips", 26, "", true, "Fish & Chips", 15, 10.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 58, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Spaghetti with marinara", 27, "", true, "Spaghetti", 10, 9.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 59, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken Alfredo pasta", 28, "", true, "Fettuccine Alfredo", 12, 11.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 60, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mexican rice bowl", 29, "", true, "Burrito Bowl", 12, 9.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 61, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken stir fry", 30, "", true, "Stir Fry", 12, 10.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // Pizza Varieties
                    { 62, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Classic pepperoni pizza", 31, "", true, "Pepperoni Pizza", 15, 12.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 63, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vegetarian pizza", 32, "", true, "Veggie Pizza", 15, 11.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 64, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BBQ chicken pizza", 33, "", true, "BBQ Chicken Pizza", 15, 13.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 65, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hawaiian pizza", 34, "", true, "Hawaiian Pizza", 15, 12.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // Salads
                    { 66, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Caesar salad", 35, "", true, "Caesar Salad", 6, 7.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 67, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Greek salad", 36, "", true, "Greek Salad", 6, 6.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 68, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cobb salad", 37, "", true, "Cobb Salad", 8, 8.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 69, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken Caesar salad", 38, "", true, "Chicken Caesar", 8, 9.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 70, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quinoa bowl", 39, "", true, "Quinoa Salad", 8, 8.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // Soups
                    { 71, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Classic tomato soup", 40, "", true, "Tomato Soup", 5, 5.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 72, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Creamy chicken noodle", 41, "", true, "Chicken Noodle Soup", 5, 5.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 73, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "French onion soup", 42, "", true, "French Onion Soup", 8, 6.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 74, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Creamy mushroom soup", 43, "", true, "Mushroom Soup", 5, 5.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            // Snacks Category (Category 2) - Additional items after the existing 4
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsAvailable", "Name", "PreparationTimeMinutes", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    // More Sweet Snacks
                    { 75, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fresh baked croissant", 5, "", true, "Croissant", 2, 3.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 76, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chocolate chip muffin", 6, "", true, "Chocolate Muffin", 2, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 77, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Blueberry muffin", 7, "", true, "Blueberry Muffin", 2, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 78, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cinnamon roll with icing", 8, "", true, "Cinnamon Roll", 3, 4.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 79, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Donut assortment", 9, "", true, "Donut", 2, 2.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 80, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fresh baked brownie", 10, "", true, "Brownie", 2, 3.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 81, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cheesecake slice", 11, "", true, "Cheesecake", 3, 5.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 82, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tiramisu cup", 12, "", true, "Tiramisu", 3, 5.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 83, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ice cream scoop", 13, "", true, "Ice Cream", 2, 3.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 84, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fruit tart", 14, "", true, "Fruit Tart", 3, 4.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // Savory Snacks
                    { 85, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pretzel bites", 15, "", true, "Pretzel Bites", 2, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 86, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cheese crackers", 16, "", true, "Cheese Crackers", 1, 2.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 87, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mixed nuts", 17, "", true, "Trail Mix", 1, 2.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 88, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Popcorn", 18, "", true, "Popcorn", 2, 2.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 89, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nachos with cheese", 19, "", true, "Nachos", 3, 4.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 90, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mozzarella sticks", 20, "", true, "Mozzarella Sticks", 5, 5.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 91, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken wings", 21, "", true, "Chicken Wings", 10, 7.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 92, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Onion rings", 22, "", true, "Onion Rings", 5, 4.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },

                    // Healthy Snacks
                    { 93, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fresh apple", 23, "", true, "Apple", 1, 1.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 94, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Banana", 24, "", true, "Banana", 1, 1.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 95, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orange segments", 25, "", true, "Orange", 1, 1.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 96, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grapes", 26, "", true, "Grapes", 1, 2.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 97, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mixed berries", 27, "", true, "Berry Cup", 2, 3.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 98, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Protein bar", 28, "", true, "Protein Bar", 1, 3.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 99, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yogurt cup", 29, "", true, "Greek Yogurt", 2, 3.00m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 100, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fruit and nut bar", 30, "", true, "Energy Bar", 1, 2.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);
        }
    }
}

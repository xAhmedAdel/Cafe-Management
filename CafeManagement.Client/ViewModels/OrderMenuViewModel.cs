using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CafeManagement.Client.Services;
using CafeManagement.Client.Services.Interfaces;
using CafeManagement.Core.Enums;
using CafeManagement.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;

namespace CafeManagement.Client.ViewModels;

public partial class OrderMenuViewModel : ObservableObject
{
    private readonly ICafeManagementService _cafeService;
    private readonly ILogger<OrderMenuViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> _filteredProducts = new();

    [ObservableProperty]
    private ProductCategory _selectedCategory = ProductCategory.Drinks;

    [ObservableProperty]
    private CartDto _cart = new() { Items = new List<CartItemDto>() };

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = "";

    private Dictionary<int, int> _quantities = new();

    public OrderMenuViewModel(ICafeManagementService cafeService, ILogger<OrderMenuViewModel> logger)
    {
        _cafeService = cafeService;
        _logger = logger;

        // Initialize filtered products
        UpdateFilteredProducts();
    }

    [RelayCommand]
    public void Close()
    {
        // Close the window - this will be handled by the code-behind
    }

    [RelayCommand]
    private async Task LoadMenuAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading menu...";

            var menu = await _cafeService.GetMenuAsync();

            AllProducts.Clear();
            foreach (var product in menu.Categories.SelectMany(c => c.Products))
            {
                AllProducts.Add(product);
            }

            UpdateFilteredProducts();
            StatusMessage = $"Loaded {AllProducts.Count} menu items";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading menu");
            StatusMessage = "Failed to load menu";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedCategoryChanged(ProductCategory value)
    {
        UpdateFilteredProducts();
    }

    private void UpdateFilteredProducts()
    {
        FilteredProducts.Clear();
        var filtered = AllProducts.Where(p => p.Category == SelectedCategory);
        foreach (var product in filtered)
        {
            FilteredProducts.Add(product);
        }
    }

    [RelayCommand]
    private async Task AddToCartAsync(ProductDto? product)
    {
        if (product == null) return;

        try
        {
            var quantity = GetProductQuantity(product.Id);
            if (quantity > 0)
            {
                var cartItem = Cart.Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (cartItem != null)
                {
                    Cart.Items.Remove(cartItem);
                }

                cartItem = new CartItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl,
                    Category = product.Category
                };

                Cart.Items.Add(cartItem);

                // Reset quantity after adding to cart
                _quantities[product.Id] = 0;

                OnPropertyChanged(nameof(Cart));
                StatusMessage = $"Added {quantity}x {product.Name} to cart";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to cart");
            StatusMessage = "Failed to add to cart";
        }
    }

    [RelayCommand]
    private void IncreaseQuantity(int productId)
    {
        if (!_quantities.ContainsKey(productId))
            _quantities[productId] = 0;

        _quantities[productId]++;

        // Update the filtered products collection to refresh UI
        var product = FilteredProducts.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            var index = FilteredProducts.IndexOf(product);
            if (index >= 0)
            {
                FilteredProducts.RemoveAt(index);
                FilteredProducts.Insert(index, product);
            }
        }
    }

    [RelayCommand]
    private void DecreaseQuantity(int productId)
    {
        if (!_quantities.ContainsKey(productId))
            _quantities[productId] = 0;

        if (_quantities[productId] > 0)
            _quantities[productId]--;

        // Update the filtered products collection to refresh UI
        var product = FilteredProducts.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            var index = FilteredProducts.IndexOf(product);
            if (index >= 0)
            {
                FilteredProducts.RemoveAt(index);
                FilteredProducts.Insert(index, product);
            }
        }
    }

    private int GetProductQuantity(int productId)
    {
        return _quantities.TryGetValue(productId, out var quantity) ? quantity : 0;
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!Cart.Items.Any())
        {
            StatusMessage = "Cart is empty";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Placing order...";

            var orderRequest = new CreateOrderRequest
            {
                Items = Cart.Items.Select(item => new CreateOrderItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList(),
                CustomerNotes = "Order from client application"
            };

            var result = await _cafeService.CreateOrderAsync(orderRequest);

            if (result)
            {
                StatusMessage = "Order placed successfully!";
                Cart.Items.Clear();
                _quantities.Clear();
                OnPropertyChanged(nameof(Cart));

                // Close window after successful order
                await Task.Delay(2000);
                Close();
            }
            else
            {
                StatusMessage = "Failed to place order";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing order");
            StatusMessage = "Failed to place order";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Globalization;
using System.Windows.Data;
using CafeManagement.Client.ViewModels;
using CafeManagement.Core.Enums;

namespace CafeManagement.Client.Views;

public partial class OrderMenuWindow : Window
{
    private readonly OrderMenuViewModel _viewModel;

    public OrderMenuWindow(OrderMenuViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Handle window events
        this.KeyDown += OrderMenuWindow_KeyDown;
        this.Loaded += OrderMenuWindow_Loaded;
    }

    private void OrderMenuWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Load the menu when window opens
        _viewModel.LoadMenuCommand?.Execute(null);
    }

    private void OrderMenuWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Category_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button)
        {
            var category = button.Content.ToString() switch
            {
                "☕ Drinks" => ProductCategory.Drinks,
                "🍔 Food" => ProductCategory.Food,
                "🍿 Snacks" => ProductCategory.Snacks,
                _ => ProductCategory.Drinks
            };
            _viewModel.SelectedCategory = category;
        }
    }
}

/// <summary>
/// Converter to convert enum values to boolean for RadioButton binding
/// </summary>
public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return Enum.Parse(targetType, parameter.ToString(), true);
        }
        return System.Windows.Data.Binding.DoNothing;
    }
}

/// <summary>
/// Converter to convert count to visibility - shows when count > 0
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
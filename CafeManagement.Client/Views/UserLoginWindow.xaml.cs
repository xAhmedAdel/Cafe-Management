using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CafeManagement.Client.ViewModels;

namespace CafeManagement.Client.Views;

public partial class UserLoginWindow : Window
{
    private readonly UserLoginViewModel _viewModel;
    private readonly System.Timers.Timer _timeUpdateTimer;

    public UserLoginWindow(UserLoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        // Setup time update timer
        _timeUpdateTimer = new System.Timers.Timer(1000); // Update every second
        _timeUpdateTimer.Elapsed += UpdateCurrentTime;
        _timeUpdateTimer.Start();

        // Handle window events
        Loaded += UserLoginWindow_Loaded;
        KeyDown += UserLoginWindow_KeyDown;
    }

    private async void UserLoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Initialize the view model
            await _viewModel.InitializeAsync();

            // Setup time update for session countdown
            var countdownTimer = new System.Timers.Timer(1000);
            countdownTimer.Elapsed += (s, args) => Dispatcher.Invoke(() => _viewModel.UpdateTimeRemaining());
            countdownTimer.Start();
        }
        catch (Exception ex)
        {
            // Log error or handle appropriately
            System.Diagnostics.Debug.WriteLine($"Error initializing login window: {ex.Message}");
        }
    }

    private void UserLoginWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Allow Alt+F4 to close the window
        if (e.Key == Key.F4 && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            return;
        }

        // Prevent other keyboard shortcuts from closing the window
        if (e.Key == Key.F4 && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            e.Handled = true;
        }

        // Allow Enter key to trigger login
        if (e.Key == Key.Enter && _viewModel.CanLogin)
        {
            _viewModel.LoginCommand.Execute(null);
            e.Handled = true;
        }

        // Allow Escape key to close (if not logged in)
        if (e.Key == Key.Escape && !_viewModel.IsUserLoggedIn)
        {
            Close();
            e.Handled = true;
        }
    }

    private void UpdateCurrentTime(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Update current time display if needed
            var currentTimeText = FindName("CurrentTimeText") as TextBlock;
            if (currentTimeText != null)
            {
                currentTimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        });
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.Password = passwordBox.Password;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Prevent closing if user is logged in (unless explicitly allowed)
        if (_viewModel.IsUserLoggedIn)
        {
            var result = System.Windows.MessageBox.Show(
                "You have an active session. Are you sure you want to exit?",
                "Active Session",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        // Stop timers
        _timeUpdateTimer?.Stop();
        _timeUpdateTimer?.Dispose();

        base.OnClosing(e);
    }

    // Add value converters for bindings
    public static class Converters
    {
        public static bool InverseBool(bool value) => !value;

        public static Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility InverseBoolToVisibility(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

        public static System.Windows.Media.Color BoolToColor(bool isConnected) => isConnected ? System.Windows.Media.Colors.Green : System.Windows.Media.Colors.Red;

        public static Transform WidthToScale(double width) => new ScaleTransform(width / 20.0, 1.0);

        public static Visibility StringToVisibility(string value) => string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
    }
}
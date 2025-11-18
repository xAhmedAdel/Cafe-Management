using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Data;
using CafeManagement.Client.ViewModels;
using CafeManagement.Client.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CafeManagement.Client.Views;

public partial class LockScreenWindow : Window
{
    private readonly LockScreenViewModel _viewModel;
    private readonly UserLoginViewModel _userLoginViewModel;
    private readonly ISystemTrayService _systemTrayService;
    private readonly DispatcherTimer _clockTimer;
    private readonly Random _random = new();
    private readonly List<Ellipse> _particles = new();
    private UserDashboardWindow? _dashboardWindow;
    private bool _isTransitioning = false;

    public UserLoginViewModel UserLoginViewModel => _userLoginViewModel;

    public LockScreenWindow(LockScreenViewModel viewModel, ISystemTrayService systemTrayService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _systemTrayService = systemTrayService;

        // Create UserLoginViewModel with required services
        var serviceProvider = ((App)System.Windows.Application.Current).ServiceProvider;
        _userLoginViewModel = serviceProvider.GetRequiredService<UserLoginViewModel>();

        // Subscribe to login success event only
        _userLoginViewModel.UserLoginSuccess += (sender, args) =>
        {
            System.Diagnostics.Debug.WriteLine("UserLoginSuccess event received in LockScreenWindow");
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("About to call ShowDashboardWindow from UserLoginSuccess handler");
                // Close lockscreen and show dashboard
                ShowDashboardWindow();
                System.Diagnostics.Debug.WriteLine("ShowDashboardWindow called successfully");
            });
        };

        _userLoginViewModel.HideDashboardRequested += (sender, args) =>
        {
            Dispatcher.Invoke(() =>
            {
                HideDashboardWindow();
            });
        };

        DataContext = _viewModel;

        // Initialize system tray service
        _systemTrayService.Initialize();
        _systemTrayService.SetMainWindow(this);

        // Clock timer for updating time display
        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += ClockTimer_Tick;
        _clockTimer.Start();

        // Start particle animation
        StartParticleAnimation();

        // Start background animation
        StartBackgroundAnimation();

        // Handle window events
        KeyDown += LockScreenWindow_KeyDown;
        Loaded += LockScreenWindow_Loaded;

        // Test lock screen on Ctrl+L for testing purposes
        KeyDown += (sender, e) =>
        {
            if (e.Key == System.Windows.Input.Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                TestLockScreen();
            }
        };
    }

    private async void LockScreenWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize UserLoginViewModel
        await _userLoginViewModel.InitializeAsync();

        // Animate entrance
        var scaleAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(0.5),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(OpacityProperty, scaleAnimation);

        await _viewModel.InitializeAsync();
    }

    private async void ClockTimer_Tick(object? sender, EventArgs e)
    {
        _viewModel.UpdateCurrentTime();
        await _viewModel.UpdateTimeRemaining();
    }

    private void LockScreenWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Ctrl+Alt+Del for admin unlock
        if (e.Key == Key.Delete && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
            (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            _viewModel.AdminUnlockCommand.Execute(null);
        }
    }

    private void StartParticleAnimation()
    {
        // Defer particle creation until window is loaded
        Loaded += (s, e) =>
        {
            for (int i = 0; i < 50; i++)
            {
                CreateParticle();
            }

            DispatcherTimer particleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            particleTimer.Tick += (s, e) => UpdateParticles();
            particleTimer.Start();
        };
    }

    private void CreateParticle()
    {
        if (ParticleCanvas == null || ActualWidth <= 0 || ActualHeight <= 0)
            return;

        var particle = new Ellipse
        {
            Width = _random.Next(2, 8),
            Height = _random.Next(2, 8),
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 255))
        };

        Canvas.SetLeft(particle, _random.Next(0, (int)ActualWidth));
        Canvas.SetTop(particle, _random.Next(0, (int)ActualHeight));

        ParticleCanvas.Children.Add(particle);
        _particles.Add(particle);
    }

    private void UpdateParticles()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        foreach (var particle in _particles)
        {
            var currentTop = Canvas.GetTop(particle);
            var currentLeft = Canvas.GetLeft(particle);

            // Floating motion
            var newTop = currentTop - (0.5 + _random.NextDouble() * 1.5);
            var newLeft = currentLeft + (_random.NextDouble() * 2.0 - 1.0);

            if (newTop < -10)
            {
                newTop = ActualHeight;
                newLeft = _random.Next(0, (int)ActualWidth);
            }

            if (newLeft < -10)
            {
                newLeft = ActualWidth;
            }
            else if (newLeft > ActualWidth)
            {
                newLeft = 0;
            }

            Canvas.SetTop(particle, newTop);
            Canvas.SetLeft(particle, newLeft);
        }
    }

    private void StartBackgroundAnimation()
    {
        // Defer background animation until window is loaded
        Loaded += (s, e) =>
        {
            if (BackgroundGradient != null)
            {
                var animation = new ColorAnimation
                {
                    From = Colors.DarkBlue,
                    To = Colors.DarkSlateBlue,
                    Duration = TimeSpan.FromSeconds(5),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                ((GradientStop)BackgroundGradient.GradientStops[0]).BeginAnimation(GradientStop.ColorProperty, animation);

                var animation2 = new ColorAnimation
                {
                    From = Colors.DarkSlateBlue,
                    To = Colors.DarkBlue,
                    Duration = TimeSpan.FromSeconds(5),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                ((GradientStop)BackgroundGradient.GradientStops[1]).BeginAnimation(GradientStop.ColorProperty, animation2);
            }
        };
    }

    private void TestLockScreen()
    {
        try
        {
            System.Windows.MessageBox.Show("Testing lock screen functionality!", "Test", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Test failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            _clockTimer?.Stop();
            _systemTrayService?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping clock timer: {ex.Message}");
        }
        base.OnClosed(e);
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            ShowInTaskbar = false;
            _systemTrayService?.Show();
            _systemTrayService?.ShowBalloonTip("Cafe Management Client", "Application minimized to system tray", ToolTipIcon.Info);
            _systemTrayService?.UpdateToolTip("Cafe Management - Minimized");
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Add keyboard handler for minimizing
        this.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Escape && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                WindowState = WindowState.Minimized;
                e.Handled = true;
            }
        };
    }

    public void ShowLockScreen()
    {
        Show();
        WindowState = WindowState.Maximized;
        Topmost = true;
    }

    public void HideLockScreen()
    {
        Hide();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && _userLoginViewModel != null)
        {
            _userLoginViewModel.Password = passwordBox.Password;
        }
    }

    public void SlidePanelIn()
    {
        var storyboard = (Storyboard)FindResource("SlidePanelIn");
        storyboard.Begin();
    }

    public void SlidePanelOut()
    {
        var storyboard = (Storyboard)FindResource("SlidePanelOut");
        storyboard.Begin();
    }

    private void ShowDashboardWindow()
    {
        try
        {
            // Prevent multiple calls
            if (_isTransitioning)
            {
                System.Diagnostics.Debug.WriteLine("Already transitioning to dashboard, skipping duplicate call");
                return;
            }

            _isTransitioning = true;

            // Create dashboard window if needed
            if (_dashboardWindow == null)
            {
                _dashboardWindow = new UserDashboardWindow(_userLoginViewModel);
            }

            // Set dashboard to take full height and 400px width, positioned on right side
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            _dashboardWindow.Left = screenWidth - 400; // Start from right edge
            _dashboardWindow.Top = 0;
            _dashboardWindow.Width = 400;
            _dashboardWindow.Height = screenHeight;
            _dashboardWindow.WindowState = WindowState.Normal;

            // Show dashboard first with proper activation
            _dashboardWindow.Show();
            _dashboardWindow.Topmost = true; // Temporarily make topmost to ensure it appears
            _dashboardWindow.BringIntoView();
            _dashboardWindow.Focus();
            _dashboardWindow.Activate();

            // Wait a bit then remove topmost and hide lockscreen
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _dashboardWindow.Topmost = false;

                    // Clear lockscreen display AFTER dashboard is visible
                    _viewModel.ClearSessionDisplay();

                    // Hide lockscreen completely
                    HideLockScreen();

                    // Update system tray
                    _systemTrayService.UpdateToolTip("Cafe Management - User Logged In");

                    System.Diagnostics.Debug.WriteLine("Dashboard transition completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in dashboard transition: {ex.Message}");
                }
                finally
                {
                    _isTransitioning = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            _isTransitioning = false;
            System.Diagnostics.Debug.WriteLine($"Error showing dashboard: {ex.Message}");
        }
    }

    private void HideDashboardWindow()
    {
        try
        {
            _dashboardWindow?.Hide();
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Error hiding dashboard: {ex.Message}");
        }
    }
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected ? Colors.LimeGreen : Colors.Red;
        }
        return Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.Red);
        }
        return new SolidColorBrush(Colors.Red);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return true;
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
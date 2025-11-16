using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CafeManagement.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CafeManagement.Client.Views;

public partial class LockScreenWindow : Window
{
    private readonly LockScreenViewModel _viewModel;
    private readonly DispatcherTimer _clockTimer;
    private readonly Random _random = new();
    private readonly List<Ellipse> _particles = new();

    public LockScreenWindow(LockScreenViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

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
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                TestLockScreen();
            }
        };
    }

    private async void LockScreenWindow_Loaded(object sender, RoutedEventArgs e)
    {
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

    private void LockScreenWindow_KeyDown(object sender, KeyEventArgs e)
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
            Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255))
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
            MessageBox.Show("Testing lock screen functionality!", "Test", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Test failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            _clockTimer?.Stop();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping clock timer: {ex.Message}");
        }
        base.OnClosed(e);
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
}
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using CafeManagement.Client.Services;
using CafeManagement.Client.Services.Interfaces;
using CafeManagement.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CafeManagement.Client.ViewModels;

public partial class UserLoginViewModel : ObservableObject, IDisposable
{
    public event EventHandler? UserLoginSuccess;
    public event EventHandler? HideDashboardRequested;

    private readonly IUserSessionService _userSessionService;
    private readonly ICafeManagementService _cafeService;
    private readonly ILockScreenService _lockScreenService;
    private readonly ISystemTrayService _systemTrayService;
    private readonly ILogger<UserLoginViewModel> _logger;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _statusMessage = "Please enter your credentials";

    [ObservableProperty]
    private bool _isLoggingIn = false;

    [ObservableProperty]
    private bool _canLogin = true;

    [ObservableProperty]
    private bool _showTimeRemaining = false;

    [ObservableProperty]
    private string _timeRemainingFormatted = "";

    [ObservableProperty]
    private string _currentUserName = "";

    [ObservableProperty]
    private string _sessionStartTimeFormatted = "";


    [ObservableProperty]
    private decimal _hourlyRate = 0.00m;

    [ObservableProperty]
    private decimal _accountBalance = 0.00m;

    [ObservableProperty]
    private decimal _currentCost = 0.00m;

    [ObservableProperty]
    private string _usageTimeFormatted = "00:00:00";

    [ObservableProperty]
    private string _connectionStatus = "Connecting...";

    [ObservableProperty]
    private bool _isConnected = false;

    [ObservableProperty]
    private string _clientInfo = "";

    [ObservableProperty]
    private bool _isUserLoggedIn = false;

    private int? _clientId;
    private readonly Timer? _updateTimer;
    private DateTime _sessionStartTime;

    public UserLoginViewModel(
        IUserSessionService userSessionService,
        ICafeManagementService cafeService,
        ILockScreenService lockScreenService,
        ISystemTrayService systemTrayService,
        ILogger<UserLoginViewModel> logger)
    {
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        _cafeService = cafeService ?? throw new ArgumentNullException(nameof(cafeService));
        _lockScreenService = lockScreenService ?? throw new ArgumentNullException(nameof(lockScreenService));
        _systemTrayService = systemTrayService ?? throw new ArgumentNullException(nameof(systemTrayService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Setup update timer (updates every second)
        _updateTimer = new Timer(1000);
        _updateTimer.Elapsed += OnUpdateTimerElapsed;
        _updateTimer.AutoReset = true;

        SetupEventHandlers();
        UpdateClientInfo();
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        _logger.LogInformation("Login button clicked! Username: '{Username}', Has Password: {HasPassword}", Username, !string.IsNullOrEmpty(Password));

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            _logger.LogWarning("Login failed: Empty username or password");
            StatusMessage = "Please enter both username and password";
            return;
        }

        try
        {
            _logger.LogInformation("Starting login process for user: {Username}", Username);
            IsLoggingIn = true;
            CanLogin = false;
            StatusMessage = "Logging in...";

            if (_clientId == null)
            {
                _logger.LogError("Login failed: Client ID is null");
                StatusMessage = "Error: Client not registered";
                return;
            }

            System.Diagnostics.Debug.WriteLine("DEBUG: About to call _userSessionService.LoginUserAsync");
            var success = await _userSessionService.LoginUserAsync(Username, Password, _clientId.Value);
            System.Diagnostics.Debug.WriteLine($"DEBUG: _userSessionService.LoginUserAsync returned: {success}");

            _logger.LogInformation("Login result for user {Username}: {Success}", Username, success);
            System.Diagnostics.Debug.WriteLine("DEBUG: Reached login result processing");
            System.Diagnostics.Debug.WriteLine($"Login success value: {success}");

            if (success)
            {
                _logger.LogInformation("Login successful for user: {Username}", Username);
                Password = ""; // Clear password for security
                StatusMessage = $"Welcome, {_userSessionService.CurrentUser?.Username}!";

                if (_userSessionService.CurrentSession != null)
                {
                    var session = _userSessionService.CurrentSession;
                    CurrentUserName = _userSessionService.CurrentUser?.Username ?? "";
                    SessionStartTimeFormatted = session.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    HourlyRate = session.HourlyRate;
                    ShowTimeRemaining = true;
                }

                // Force immediate UI transition - don't wait for events
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Hide any lockscreen windows directly
                        foreach (Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window is LockScreenWindow)
                            {
                                window.Hide();
                                break;
                            }
                        }

                        // Create and show dashboard window
                        var dashboardWindow = new UserDashboardWindow(this);
                        // Set dashboard to take full height and 400px width, positioned on right side
                        var screenHeight = SystemParameters.PrimaryScreenHeight;
                        var screenWidth = SystemParameters.PrimaryScreenWidth;
                        dashboardWindow.Left = screenWidth - 400; // Start from right edge
                        dashboardWindow.Top = 0;
                        dashboardWindow.Width = 400;
                        dashboardWindow.Height = screenHeight;
                        dashboardWindow.WindowState = WindowState.Normal;
                        dashboardWindow.Show();
                        dashboardWindow.Activate();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error transitioning to dashboard");
                    }
                });

                // Also trigger the event as backup
                UserLoginSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.LogWarning("Login failed for user: {Username}", Username);
                StatusMessage = "Login failed. Please check your credentials.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            StatusMessage = "Login error. Please try again.";
        }
        finally
        {
            IsLoggingIn = false;
            CanLogin = true;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            StatusMessage = "Logging out...";
            await _userSessionService.LogoutUserAsync();

            Username = "";
            Password = "";
            ShowTimeRemaining = false;
            TimeRemainingFormatted = "";
            CurrentUserName = "";
            SessionStartTimeFormatted = "";
            HourlyRate = 0.00m;
            StatusMessage = "Logged out successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            StatusMessage = "Logout error. Please try again.";
        }
    }

    [RelayCommand]
    private async Task ExtendSessionAsync()
    {
        if (_userSessionService.CurrentSession == null)
            return;

        try
        {
            StatusMessage = "Extending session...";
            await _userSessionService.ExtendSessionAsync(30); // Extend by 30 minutes
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session");
            StatusMessage = "Error extending session.";
        }
    }

    [RelayCommand]
    private async Task RequestUnlockAsync()
    {
        try
        {
            if (await _cafeService.RequestUnlockAsync())
            {
                StatusMessage = "Unlock request sent to administrator";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting unlock");
            StatusMessage = "Error requesting unlock.";
        }
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("UserLoginViewModel InitializeAsync started");
            StatusMessage = "Initializing client...";

            // Register client with server
            _logger.LogInformation("Registering client with server");
            await _cafeService.RegisterClientAsync();

            // Get client ID for login
            _logger.LogInformation("Getting current client information");
            var clientDto = await _cafeService.GetCurrentClientAsync();
            if (clientDto != null)
            {
                _clientId = clientDto.Id;
                _logger.LogInformation("Client registered successfully with ID: {ClientId}", _clientId);

                // Check if there's an existing active session for this client
                _logger.LogInformation("Checking for existing active session for client {ClientId}", _clientId);
                var existingSession = await _cafeService.GetActiveSessionAsync(_clientId.Value);

                if (existingSession != null)
                {
                    var username = existingSession.User?.Username ?? "Unknown";
                    _logger.LogInformation("Found existing active session for user: {Username}, ending session to start fresh", username);

                    // End the existing session and save time
                    try
                    {
                        await _cafeService.EndSessionAsync(existingSession.Id);
                        _logger.LogInformation("Previous session ended successfully for user: {Username}", username);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error ending previous session for user: {Username}", username);
                    }
                }

                // Always show login form after ending any existing session
                StatusMessage = "Ready for login";
                ConnectionStatus = "Connected";
                IsConnected = true;

                // Clear any previous session display data
                ShowTimeRemaining = false;
                TimeRemainingFormatted = "";
                CurrentUserName = "";
                SessionStartTimeFormatted = "";
                HourlyRate = 0.00m;
                AccountBalance = 0.00m;
                CurrentCost = 0.00m;
                UsageTimeFormatted = "00:00:00";
                IsUserLoggedIn = false;
            }
            else
            {
                _logger.LogWarning("GetCurrentClientAsync returned null - client registration failed");
                StatusMessage = "Error: Could not register client";
                ConnectionStatus = "Connection failed";
                IsConnected = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing login view model");
            StatusMessage = $"Initialization error: {ex.Message}";
            ConnectionStatus = "Connection failed";
            IsConnected = false;
        }
    }

    private void SetupEventHandlers()
    {
        _userSessionService.UserLoggedIn += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsUserLoggedIn = true;
                if (args.Session != null)
                {
                    _sessionStartTime = args.Session.StartTime;
                    SessionStartTimeFormatted = args.Session.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    HourlyRate = args.Session.HourlyRate;
                    AccountBalance = args.Session.User?.Balance ?? 0.00m;
                }
                _updateTimer?.Start();
            });
        };

        _userSessionService.UserLoggedOut += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsUserLoggedIn = false;
                ShowTimeRemaining = false;
                _updateTimer?.Stop();
                CurrentCost = 0.00m;
                UsageTimeFormatted = "00:00:00";
            });
        };

        _userSessionService.TimeWarning += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = args.Message;
            });
        };

        _userSessionService.SessionExpired += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = "Session expired! Logging out...";
                ShowTimeRemaining = false;
            });
        };

        _userSessionService.SessionStatusChanged += (sender, message) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = message;
            });
        };
    }

    private void UpdateClientInfo()
    {
        try
        {
            var computerName = Environment.MachineName;
            var localIp = "127.0.0.1"; // This should come from system service
            ClientInfo = $"{computerName} - {localIp}";
        }
        catch
        {
            ClientInfo = "Client Computer";
        }
    }

    partial void OnUsernameChanged(string value)
    {
        CanLogin = !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Password) && !IsLoggingIn;
    }

    partial void OnPasswordChanged(string value)
    {
        CanLogin = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(value) && !IsLoggingIn;
    }

    [RelayCommand]
    private void HideToTray()
    {
        try
        {
            _logger.LogInformation("Hiding application to system tray");
            _systemTrayService.Hide();
            HideDashboardRequested?.Invoke(this, EventArgs.Empty);
            StatusMessage = "Application hidden to system tray";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding application to system tray");
        }
    }

    public void UpdateTimeRemaining()
    {
        if (_userSessionService.TimeRemaining.HasValue)
        {
            var remaining = _userSessionService.TimeRemaining.Value;
            TimeRemainingFormatted = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }

    private void OnUpdateTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!IsUserLoggedIn)
            return;

        try
        {
            // Update time remaining
            UpdateTimeRemaining();

            // Calculate usage time
            var usageDuration = DateTime.UtcNow - _sessionStartTime;
            UsageTimeFormatted = $"{usageDuration.Hours:D2}:{usageDuration.Minutes:D2}:{usageDuration.Seconds:D2}";

            // Calculate current cost based on usage time and hourly rate
            var cost = (usageDuration.TotalHours * (double)HourlyRate);
            CurrentCost = Math.Round((decimal)cost, 2);

            // Update account balance from user session if available
            if (_userSessionService.CurrentUser != null)
            {
                AccountBalance = _userSessionService.CurrentUser.Balance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating timer display");
        }
    }

    public void Dispose()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
    }
}
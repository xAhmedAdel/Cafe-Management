using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CafeManagement.Client.Services;
using CafeManagement.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafeManagement.Client.ViewModels;

public partial class LockScreenViewModel : ObservableObject
{
    private readonly ICafeManagementService _cafeService;
    private readonly ISystemService _systemService;
    private readonly ISignalRService _signalRService;
    private readonly IUserSessionService _userSessionService;

    [ObservableProperty]
    private string _clientName = "Client Computer";

    [ObservableProperty]
    private string _statusMessage = "Computer locked by administrator";

    [ObservableProperty]
    private string _timeRemainingFormatted = "00:00:00";

    [ObservableProperty]
    private int _timeRemainingMinutes = 0;

    [ObservableProperty]
    private string _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [ObservableProperty]
    private string _clientInfo = "";

    [ObservableProperty]
    private string _sessionStartTime = "";

    [ObservableProperty]
    private decimal _hourlyRate = 2.00m;

    [ObservableProperty]
    private bool _hasSessionInfo = false;

    [ObservableProperty]
    private bool _canUnlock = false;

    [ObservableProperty]
    private bool _canExit = false;

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _connectionStatus = "Initializing...";

    [ObservableProperty]
    private bool _isConnected = false;

    private DateTime _sessionEndTime;
    private System.Threading.Timer? _countdownTimer;

    public LockScreenViewModel(ICafeManagementService cafeService, ISystemService systemService, ISignalRService signalRService, IUserSessionService userSessionService)
    {
        _cafeService = cafeService ?? throw new ArgumentNullException(nameof(cafeService));
        _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        _signalRService = signalRService ?? throw new ArgumentNullException(nameof(signalRService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));

        UpdateClientInfo();
        SetupSignalREventHandlers();
        SetupUserSessionHandlers();
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        if (await _cafeService.RequestUnlockAsync())
        {
            await _signalRService.NotifyUnlockRequested();
            // The server will send unlock command via SignalR
        }
    }

    [RelayCommand]
    private async Task AdminUnlockAsync()
    {
        await _cafeService.AdminUnlockAsync();
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        await _systemService.ExitApplication();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Connect to SignalR
            await _signalRService.ConnectAsync();

            // Get current session info from UserSessionService
            if (_userSessionService.CurrentSession != null)
            {
                var userSession = _userSessionService.CurrentSession;
                _sessionEndTime = userSession.EndTime ?? DateTime.UtcNow.AddMinutes(userSession.DurationMinutes);
                _sessionStartTime = userSession.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                _hourlyRate = userSession.HourlyRate;
                _hasSessionInfo = true;

                if (_sessionEndTime > DateTime.UtcNow)
                {
                    StartCountdown();
                }
            }
            else
            {
                // Fallback to cafe service session
                var currentSession = await _cafeService.GetCurrentSessionAsync();
                if (currentSession != null)
                {
                    _sessionEndTime = currentSession.EndTime ?? DateTime.UtcNow.AddMinutes(currentSession.DurationMinutes);
                    _sessionStartTime = currentSession.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    _hourlyRate = currentSession.HourlyRate;
                    _hasSessionInfo = true;

                    if (_sessionEndTime > DateTime.UtcNow)
                    {
                        StartCountdown();
                    }
                }
            }

            // Register client with server
            await _cafeService.RegisterClientAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public void UpdateCurrentTime()
    {
        CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public async Task UpdateTimeRemaining()
    {
        if (_sessionEndTime > DateTime.UtcNow)
        {
            var remaining = _sessionEndTime - DateTime.UtcNow;
            TimeRemainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
            TimeRemainingFormatted = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";

            if (remaining.TotalMinutes <= 1)
            {
                StatusMessage = "Session ending soon!";
            }
        }
        else if (_sessionEndTime != DateTime.MinValue)
        {
            TimeRemainingFormatted = "00:00:00";
            TimeRemainingMinutes = 0;
            StatusMessage = "Session ended";
            StopCountdown();
            _hasSessionInfo = false;
        }
    }

    private void StartCountdown()
    {
        StopCountdown();
        _countdownTimer = new System.Threading.Timer(state => _ = UpdateTimeRemaining(), null, 0, 1000);
    }

    private void StopCountdown()
    {
        _countdownTimer?.Dispose();
        _countdownTimer = null;
    }

    private void UpdateClientInfo()
    {
        try
        {
            var computerName = Environment.MachineName;
            var localIp = _systemService?.GetLocalIpAddress() ?? "127.0.0.1";
            ClientInfo = $"{computerName} - {localIp}";
        }
        catch
        {
            ClientInfo = "Client Computer";
        }
    }

    public void SetSessionInfo(DateTime endTime, decimal rate)
    {
        _sessionEndTime = endTime;
        _hourlyRate = rate;
        _hasSessionInfo = true;
        StartCountdown();
    }

    public void SetTimeWarning(int minutes)
    {
        if (minutes <= 5)
        {
            StatusMessage = $"Warning: Only {minutes} minute{(minutes != 1 ? "s" : "")} remaining!";
        }
    }

    public void SetUnlockPermission(bool canUnlock)
    {
        CanUnlock = canUnlock;
    }

    public void SetExitPermission(bool canExit)
    {
        CanExit = canExit;
    }

    public void ShowCustomMessage(string message)
    {
        StatusMessage = message;
    }

    partial void OnTimeRemainingMinutesChanged(int value)
    {
        if (value <= 5 && value > 0)
        {
            SetTimeWarning(value);
        }
    }

    private void SetupSignalREventHandlers()
    {
        if (_signalRService is SignalRService signalRService)
        {
            signalRService.ConnectionStatusChanged += (sender, args) =>
            {
                ConnectionStatus = args.Status;
                IsConnected = args.IsConnected;
            };
        }
    }

    private void SetupUserSessionHandlers()
    {
        // Handle user session events
        _userSessionService.UserLoggedIn += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = $"User logged in: {args.User?.Username}";
                if (args.Session != null)
                {
                    _sessionStartTime = args.Session.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    _hourlyRate = args.Session.HourlyRate;
                    _hasSessionInfo = true;
                    StartCountdown();
                }
            });
        };

        _userSessionService.UserLoggedOut += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = "User logged out";
                _hasSessionInfo = false;
                StopCountdown();
            });
        };

        _userSessionService.TimeWarning += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SetTimeWarning(args.MinutesRemaining);
                StatusMessage = args.Message;
            });
        };

        _userSessionService.SessionExpired += (sender, args) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = "Session expired - Computer locked";
                _hasSessionInfo = false;
                StopCountdown();
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

    public void ClearSessionDisplay()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            HasSessionInfo = false;
            SessionStartTime = "";
            HourlyRate = 0.00m;
            TimeRemainingMinutes = 0;
            TimeRemainingFormatted = "00:00:00";
            StatusMessage = "Ready for login";
            StopCountdown();
        });
    }
}
using Microsoft.Extensions.Logging;
using CafeManagement.Client.Services.Interfaces;
using CafeManagement.Application.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CafeManagement.Client.Services;

public interface IUserSessionService
{
    event EventHandler<UserSessionEventArgs>? UserLoggedIn;
    event EventHandler<UserSessionEventArgs>? UserLoggedOut;
    event EventHandler<TimeWarningEventArgs>? TimeWarning;
    event EventHandler? SessionExpired;
    event EventHandler<string>? SessionStatusChanged;

    UserDto? CurrentUser { get; }
    SessionDto? CurrentSession { get; }
    bool IsUserLoggedIn { get; }
    bool IsSessionActive { get; }
    TimeSpan? TimeRemaining { get; }

    Task<bool> LoginUserAsync(string username, string password, int clientId);
    Task LogoutUserAsync();
    Task RefreshSessionStatusAsync();
    Task ExtendSessionAsync(int additionalMinutes);
    Task RestoreSessionAsync(SessionDto session);
    void StartSessionMonitoring();
    void StopSessionMonitoring();
}

public class UserSessionService : IUserSessionService, IDisposable
{
    private readonly ILogger<UserSessionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ISignalRService _signalRService;
    private readonly ICafeManagementService _cafeService;
    private readonly Timer? _monitoringTimer;
    private readonly Timer? _countdownTimer;

    private UserDto? _currentUser;
    private SessionDto? _currentSession;
    private DateTime _sessionEndTime;
    private bool _isMonitoring;

    public event EventHandler<UserSessionEventArgs>? UserLoggedIn;
    public event EventHandler<UserSessionEventArgs>? UserLoggedOut;
    public event EventHandler<TimeWarningEventArgs>? TimeWarning;
    public event EventHandler? SessionExpired;
    public event EventHandler<string>? SessionStatusChanged;

    public UserDto? CurrentUser => _currentUser;
    public SessionDto? CurrentSession => _currentSession;
    public bool IsUserLoggedIn => _currentUser != null;
    public bool IsSessionActive => _currentSession != null && _sessionEndTime > DateTime.UtcNow;
    public TimeSpan? TimeRemaining => IsSessionActive ? _sessionEndTime - DateTime.UtcNow : null;

    private readonly string _serverBaseUrl;

    public UserSessionService(
        ILogger<UserSessionService> logger,
        HttpClient httpClient,
        ISignalRService signalRService,
        ICafeManagementService cafeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _signalRService = signalRService ?? throw new ArgumentNullException(nameof(signalRService));
        _cafeService = cafeService ?? throw new ArgumentNullException(nameof(cafeService));

        _serverBaseUrl = "http://localhost:5032"; // Should come from configuration

        // Setup monitoring timer (checks every 30 seconds)
        _monitoringTimer = new Timer(30000);
        _monitoringTimer.Elapsed += OnMonitoringTimerElapsed;

        // Setup countdown timer (updates every second)
        _countdownTimer = new Timer(1000);
        _countdownTimer.Elapsed += OnCountdownTimerElapsed;

        SetupSignalRHandlers();
    }

    public async Task<bool> LoginUserAsync(string username, string password, int clientId)
    {
        try
        {
            _logger.LogInformation($"Attempting user login: {username}");

            var loginRequest = new
            {
                Username = username,
                Password = password,
                ClientId = clientId
            };

            var response = await _httpClient.PostAsJsonAsync($"{_serverBaseUrl}/api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
                if (loginResponse?.Success == true && loginResponse.User != null && loginResponse.Session != null)
                {
                    _currentUser = loginResponse.User;
                    _currentSession = loginResponse.Session;
                    _sessionEndTime = _currentSession.EndTime ?? DateTime.UtcNow.AddMinutes(_currentSession.DurationMinutes);

                    _logger.LogInformation($"User logged in successfully: {_currentUser.Username}, Session: {_currentSession.Id}");

                    // Notify server of session start
                    await _signalRService.NotifySessionStarted(_currentSession);

                    // Start session monitoring
                    StartSessionMonitoring();

                    // Raise events
                    UserLoggedIn?.Invoke(this, new UserSessionEventArgs
                    {
                        User = _currentUser,
                        Session = _currentSession,
                        TimeRemaining = TimeRemaining
                    });

                    SessionStatusChanged?.Invoke(this, $"Logged in as {_currentUser.Username}");

                    return true;
                }
            }

            _logger.LogWarning($"Login failed for user: {username}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during user login: {username}");
            SessionStatusChanged?.Invoke(this, $"Login error: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutUserAsync()
    {
        try
        {
            if (_currentUser == null || _currentSession == null)
            {
                _logger.LogWarning("No active user session to logout");
                return;
            }

            _logger.LogInformation($"Logging out user: {_currentUser.Username}");

            // Stop monitoring first
            StopSessionMonitoring();

            // Notify server of session end
            await _signalRService.NotifySessionEnded(_currentSession);

            // End session on server
            var response = await _httpClient.PostAsync($"{_serverBaseUrl}/api/sessions/{_currentSession.Id}/end", null);

            // Raise logged out event
            var oldUser = _currentUser;
            var oldSession = _currentSession;

            _currentUser = null;
            _currentSession = null;

            UserLoggedOut?.Invoke(this, new UserSessionEventArgs
            {
                User = oldUser,
                Session = oldSession,
                TimeRemaining = null
            });

            SessionStatusChanged?.Invoke(this, "Logged out successfully");

            _logger.LogInformation($"User logged out successfully: {oldUser.Username}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user logout");
            SessionStatusChanged?.Invoke(this, $"Logout error: {ex.Message}");
        }
    }

    public async Task RefreshSessionStatusAsync()
    {
        try
        {
            if (_currentSession == null)
                return;

            // Get current session status from server
            var response = await _httpClient.GetAsync($"{_serverBaseUrl}/api/sessions/{_currentSession.Id}");

            if (response.IsSuccessStatusCode)
            {
                var updatedSession = await response.Content.ReadFromJsonAsync<SessionDto>();
                if (updatedSession != null)
                {
                    _currentSession = updatedSession;
                    _sessionEndTime = _currentSession.EndTime ?? DateTime.UtcNow.AddMinutes(_currentSession.DurationMinutes);

                    _logger.LogDebug($"Session status refreshed: {_currentSession.Status}, End time: {_sessionEndTime}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing session status");
        }
    }

    public async Task ExtendSessionAsync(int additionalMinutes)
    {
        try
        {
            if (_currentSession == null)
            {
                _logger.LogWarning("No active session to extend");
                return;
            }

            _logger.LogInformation($"Extending session by {additionalMinutes} minutes");

            var extendRequest = new { AdditionalMinutes = additionalMinutes };
            var response = await _httpClient.PostAsJsonAsync($"{_serverBaseUrl}/api/sessions/{_currentSession.Id}/extend", extendRequest);

            if (response.IsSuccessStatusCode)
            {
                await RefreshSessionStatusAsync();

                TimeWarning?.Invoke(this, new TimeWarningEventArgs
                {
                    MinutesRemaining = (int)TimeRemaining?.TotalMinutes,
                    Message = $"Session extended by {additionalMinutes} minutes"
                });

                SessionStatusChanged?.Invoke(this, $"Session extended by {additionalMinutes} minutes");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session");
            SessionStatusChanged?.Invoke(this, $"Error extending session: {ex.Message}");
        }
    }

    public void StartSessionMonitoring()
    {
        if (_isMonitoring || _currentSession == null)
            return;

        _logger.LogInformation("Starting session monitoring");
        _isMonitoring = true;
        _monitoringTimer?.Start();
        _countdownTimer?.Start();
    }

    public void StopSessionMonitoring()
    {
        if (!_isMonitoring)
            return;

        _logger.LogInformation("Stopping session monitoring");
        _isMonitoring = false;
        _monitoringTimer?.Stop();
        _countdownTimer?.Stop();
    }

    public async Task RestoreSessionAsync(SessionDto session)
    {
        try
        {
            var username = session.User?.Username ?? "Unknown";
            _logger.LogInformation($"Restoring session for user: {username}");

            // Use the user from session if available, otherwise create a minimal user DTO
            _currentUser = session.User ?? new UserDto
            {
                Id = session.UserId,
                Username = username,
                Email = "",
                Role = 0, // User role
                IsActive = true
            };

            _currentSession = session;
            _sessionEndTime = session.EndTime ?? DateTime.UtcNow.AddMinutes(session.DurationMinutes);

            _logger.LogInformation($"Session restored successfully: User {_currentUser.Username}, Session: {_currentSession.Id}");

            // Start session monitoring
            StartSessionMonitoring();

            // Raise events
            UserLoggedIn?.Invoke(this, new UserSessionEventArgs
            {
                User = _currentUser,
                Session = _currentSession,
                TimeRemaining = TimeRemaining
            });

            SessionStatusChanged?.Invoke(this, $"Session restored for {_currentUser.Username}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring session");
            SessionStatusChanged?.Invoke(this, $"Error restoring session: {ex.Message}");
        }
    }

    private void SetupSignalRHandlers()
    {
        // Set up SignalR event handlers to receive real-time updates from server
        // This would be expanded based on available SignalR events
    }

    private async void OnMonitoringTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await RefreshSessionStatusAsync();

        if (IsSessionActive)
        {
            _logger.LogDebug($"Session active. Time remaining: {TimeRemaining?.TotalMinutes:F1} minutes");
        }
        else
        {
            _logger.LogInformation("Session expired or ended");
            SessionExpired?.Invoke(this, EventArgs.Empty);
            StopSessionMonitoring();
        }
    }

    private void OnCountdownTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!IsSessionActive)
            return;

        var remaining = TimeRemaining;
        if (remaining.HasValue)
        {
            var totalMinutes = (int)remaining.Value.TotalMinutes;

            // Trigger time warnings at specific intervals
            if (totalMinutes == 5 || totalMinutes == 1)
            {
                TimeWarning?.Invoke(this, new TimeWarningEventArgs
                {
                    MinutesRemaining = totalMinutes,
                    Message = $"Only {totalMinutes} minute{(totalMinutes != 1 ? "s" : "")} remaining!"
                });
            }
        }
    }

    public void Dispose()
    {
        StopSessionMonitoring();
        _monitoringTimer?.Dispose();
        _countdownTimer?.Dispose();
    }
}

// Event argument classes
public class UserSessionEventArgs : EventArgs
{
    public UserDto? User { get; set; }
    public SessionDto? Session { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}

public class TimeWarningEventArgs : EventArgs
{
    public int MinutesRemaining { get; set; }
    public string Message { get; set; } = "";
}

// Response DTOs
public class UserLoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public UserDto? User { get; set; }
    public SessionDto? Session { get; set; }
}
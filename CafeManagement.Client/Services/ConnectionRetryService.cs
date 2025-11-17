using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace CafeManagement.Client.Services;

public class ConnectionRetryService : BackgroundService, IDisposable
{
    private readonly ILogger<ConnectionRetryService> _logger;
    private readonly System.Timers.Timer _retryTimer;
    private readonly object _lock = new object();
    private int _retryCount = 0;
    private bool _shouldRetry = false;
    private bool _isAttemptingConnection = false;
    private DateTime _lastRetryAttempt = DateTime.MinValue;
    private const int RETRY_INTERVAL_MS = 5000; // 5 seconds
    private Func<Task>? _connectAsyncCallback;

    public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

    public ConnectionRetryService(ILogger<ConnectionRetryService> logger)
    {
        _logger = logger;
        _retryTimer = new System.Timers.Timer(RETRY_INTERVAL_MS);
        _retryTimer.Elapsed += OnRetryTimerElapsed;
    }

    public void SetConnectCallback(Func<Task> connectAsync)
    {
        _connectAsyncCallback = connectAsync;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Connection retry service started");

        stoppingToken.Register(() =>
        {
            _logger.LogInformation("Connection retry service stopped");
        });

        return Task.CompletedTask;
    }

    public void StartRetrying()
    {
        lock (_lock)
        {
            _shouldRetry = true;
            _retryCount = 0;
            _lastRetryAttempt = DateTime.MinValue;
            _retryTimer.Start();

            _logger.LogInformation("Started connection retry mechanism");
            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
            {
                Status = "Connecting...",
                RetryCount = _retryCount,
                IsConnected = false,
                LastAttempt = DateTime.MinValue
            });
        }
    }

    public void StopRetrying()
    {
        lock (_lock)
        {
            _shouldRetry = false;
            _retryTimer.Stop();
            _retryCount = 0;

            _logger.LogInformation("Stopped connection retry mechanism");
            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
            {
                Status = "Connection monitoring stopped",
                RetryCount = _retryCount,
                IsConnected = false,
                LastAttempt = _lastRetryAttempt
            });
        }
    }

    public void OnConnected()
    {
        lock (_lock)
        {
            _shouldRetry = false;
            _isAttemptingConnection = false;
            _retryTimer.Stop();

            _logger.LogInformation($"Connection established after {_retryCount} retry attempts");
            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
            {
                Status = $"Connected to Cafe-Management",
                RetryCount = 0,
                IsConnected = true,
                LastAttempt = DateTime.UtcNow
            });
        }
    }

    public void OnDisconnected(string reason = "Unknown")
    {
        lock (_lock)
        {
            if (_shouldRetry) return;

            _shouldRetry = true;
            _retryCount = 0;
            _retryTimer.Start();

            _logger.LogInformation($"Connection lost: {reason}. Starting retry mechanism");
            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
            {
                Status = $"Connection lost. Retrying...",
                RetryCount = _retryCount,
                IsConnected = false,
                LastAttempt = DateTime.MinValue
            });
        }
    }

    private async void OnRetryTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_shouldRetry || _connectAsyncCallback == null || _isAttemptingConnection) return;

        // Prevent too frequent retry attempts
        var timeSinceLastAttempt = DateTime.UtcNow - _lastRetryAttempt;
        if (timeSinceLastAttempt.TotalMilliseconds < RETRY_INTERVAL_MS)
        {
            return;
        }

        lock (_lock)
        {
            _isAttemptingConnection = true;

            _retryCount++;
            _lastRetryAttempt = DateTime.UtcNow;

            _logger.LogInformation($"Connection retry attempt #{_retryCount}");
            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
            {
                Status = $"Connecting to Cafe-Management... (Attempt {_retryCount})",
                RetryCount = _retryCount,
                IsConnected = false,
                LastAttempt = _lastRetryAttempt
            });
        }

        try
        {
            await _connectAsyncCallback.Invoke();
            _logger.LogInformation($"Connection retry attempt #{_retryCount} completed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Connection retry attempt #{_retryCount} failed");
        }
        finally
        {
            lock (_lock)
            {
                _isAttemptingConnection = false;
            }
        }
    }

    protected virtual void OnConnectionStatusChanged(ConnectionStatusChangedEventArgs e)
    {
        ConnectionStatusChanged?.Invoke(this, e);
    }

    public new void Dispose()
    {
        _retryTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class ConnectionStatusChangedEventArgs : EventArgs
{
    public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public bool IsConnected { get; set; }
    public DateTime LastAttempt { get; set; }
}
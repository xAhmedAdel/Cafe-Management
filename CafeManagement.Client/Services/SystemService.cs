using CafeManagement.Client.Services.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CafeManagement.Client.Services;

public class SystemService : ISystemService
{
    public string GetLocalIpAddress()
    {
        try
        {
            using var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
            return endPoint?.Address.ToString() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    public string GetMacAddress()
    {
        try
        {
            var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            var firstNic = nics.FirstOrDefault(nic => nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback);
            return firstNic?.GetPhysicalAddress().ToString() ?? "00-00-00-00-00-00";
        }
        catch
        {
            return "00-00-00-00-00-00";
        }
    }

    public Task ExitApplication()
    {
        System.Windows.Application.Current.Shutdown();
        return Task.CompletedTask;
    }

    public Task RestartApplication()
    {
        var currentProcess = Process.GetCurrentProcess();
        Process.Start(currentProcess.ProcessName);
        System.Windows.Application.Current.Shutdown();
        return Task.CompletedTask;
    }

    public Task ShutdownComputer()
    {
        var psi = new ProcessStartInfo("shutdown", "/s /t 0")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);
        return Task.CompletedTask;
    }

    public Task LockWorkstation()
    {
        LockWorkStation();
        return Task.CompletedTask;
    }

    public Task UnlockWorkstation()
    {
        // This would typically be handled by Windows authentication
        // For now, we'll just close the application
        return ExitApplication();
    }

    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();
}
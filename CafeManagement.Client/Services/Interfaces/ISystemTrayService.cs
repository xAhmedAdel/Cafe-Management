using System.Windows;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagement.Client.Services.Interfaces;

public interface ISystemTrayService
{
    void Initialize();
    void SetMainWindow(Window window);
    void ShowBalloonTip(string title, string text, ToolTipIcon icon);
    void UpdateToolTip(string text);
    void Show();
    void Hide();
    void ShowMainWindow();
    void Dispose();
}
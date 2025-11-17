using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using CafeManagement.Client.ViewModels;

namespace CafeManagement.Client.Views;

public partial class UserDashboardWindow : Window, INotifyPropertyChanged
{
    private UserLoginViewModel _userLoginViewModel;

    public UserLoginViewModel UserLoginViewModel
    {
        get => _userLoginViewModel;
        set
        {
            _userLoginViewModel = value;
            OnPropertyChanged();
        }
    }

    public UserDashboardWindow(UserLoginViewModel userLoginViewModel)
    {
        InitializeComponent();
        UserLoginViewModel = userLoginViewModel;
        DataContext = this;

        // Handle window events
        this.Closing += UserDashboardWindow_Closing;
    }

    private void UserDashboardWindow_Closing(object? sender, CancelEventArgs e)
    {
        // Prevent closing - only allow hiding to tray
        e.Cancel = true;
        UserLoginViewModel?.HideToTrayCommand?.Execute(null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
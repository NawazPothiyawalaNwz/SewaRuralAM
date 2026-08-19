using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService, IToastService toastService)
    {
        _authService = authService;
        _toastService = toastService;
        Title = "Sign In";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter user name and password.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var user = await _authService.LoginAsync(UserName.Trim(), Password, RememberMe);
            if (user is null)
            {
                ErrorMessage = "Invalid user name or password.";
                _toastService.Show("Invalid user name or password.", ToastKind.Error);
                return;
            }

            _toastService.Show($"Welcome back, {user.FullName}!", ToastKind.Success);

            if (Shell.Current is AppShell appShell)
                await appShell.OnSignedInAsync(user);
            else
                await Shell.Current.GoToAsync($"//{nameof(Views.DashboardPage)}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.ForgotPasswordPage));
    }
}

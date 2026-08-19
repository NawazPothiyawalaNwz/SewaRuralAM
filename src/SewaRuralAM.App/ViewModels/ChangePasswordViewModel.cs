using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ChangePasswordViewModel(IAuthService authService, IToastService toastService)
    {
        _authService = authService;
        _toastService = toastService;
        Title = "Change Password";
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (_authService.CurrentUser is null) return;

        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "New password and confirmation do not match.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
        {
            StatusMessage = "New password must be at least 6 characters.";
            return;
        }

        var success = await _authService.ChangePasswordAsync(_authService.CurrentUser.Id, CurrentPassword, NewPassword);
        StatusMessage = success ? "Password changed successfully." : "Current password is incorrect.";
        _toastService.Show(StatusMessage, success ? ToastKind.Success : ToastKind.Error);

        if (success)
            await Shell.Current.GoToAsync("..");
    }
}

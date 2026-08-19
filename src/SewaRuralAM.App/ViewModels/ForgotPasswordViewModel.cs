using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private string userNameOrEmail = string.Empty;

    [ObservableProperty]
    private string resetToken = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool tokenGenerated;

    public ForgotPasswordViewModel(IAuthService authService, IToastService toastService)
    {
        _authService = authService;
        _toastService = toastService;
        Title = "Forgot Password";
    }

    [RelayCommand]
    private async Task RequestResetAsync()
    {
        var token = await _authService.GenerateResetTokenAsync(UserNameOrEmail.Trim());
        if (token is null)
        {
            StatusMessage = "No matching user found.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        // In production this token is emailed/SMS'd to the user; shown here since there is no messaging backend yet.
        ResetToken = token;
        TokenGenerated = true;
        StatusMessage = "Reset token generated. Enter it below with your new password.";
        _toastService.Show("Reset token generated.", ToastKind.Success);
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(ResetToken) || string.IsNullOrWhiteSpace(NewPassword))
        {
            StatusMessage = "Enter the reset token and a new password.";
            return;
        }

        var success = await _authService.ResetPasswordAsync(ResetToken.Trim(), NewPassword);
        StatusMessage = success ? "Password reset successfully. Please sign in." : "Invalid or expired reset token.";
        _toastService.Show(StatusMessage, success ? ToastKind.Success : ToastKind.Error);

        if (success)
            await Shell.Current.GoToAsync("..");
    }
}

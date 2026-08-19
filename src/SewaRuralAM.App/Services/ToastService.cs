using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.Services;

public class ToastService : IToastService
{
    public void Show(string message, ToastKind kind = ToastKind.Info)
    {
        var prefixed = kind switch
        {
            ToastKind.Success => $"✓ {message}",
            ToastKind.Error => $"✖ {message}",
            _ => message
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var toast = Toast.Make(prefixed, ToastDuration.Short, 14);
            _ = toast.Show();
        });
    }
}

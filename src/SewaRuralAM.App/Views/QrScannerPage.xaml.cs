using SewaRuralAM.App.ViewModels;
using ZXing.Net.Maui;

namespace SewaRuralAM.App.Views;

public partial class QrScannerPage : ContentPage
{
    private readonly QrScannerViewModel _viewModel;

    public QrScannerPage(QrScannerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
            await _viewModel.HandleDetectedBarcodesAsync(e.Results));
    }
}

using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class AssetQrPrintPage : ContentPage
{
    private readonly AssetQrPrintViewModel _viewModel;

    public AssetQrPrintPage(AssetQrPrintViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}

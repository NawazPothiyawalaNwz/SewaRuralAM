using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class LocationQrPrintPage : ContentPage
{
    private readonly LocationQrPrintViewModel _viewModel;

    public LocationQrPrintPage(LocationQrPrintViewModel viewModel)
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

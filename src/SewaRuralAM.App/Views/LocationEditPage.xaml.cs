using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class LocationEditPage : ContentPage
{
    private readonly LocationEditViewModel _viewModel;

    public LocationEditPage(LocationEditViewModel viewModel)
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

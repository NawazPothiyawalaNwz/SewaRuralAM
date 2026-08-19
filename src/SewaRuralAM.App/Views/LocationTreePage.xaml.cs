using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class LocationTreePage : ContentPage
{
    private readonly LocationTreeViewModel _viewModel;

    public LocationTreePage(LocationTreeViewModel viewModel)
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

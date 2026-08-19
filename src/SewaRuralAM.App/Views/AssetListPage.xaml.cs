using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class AssetListPage : ContentPage
{
    private readonly AssetListViewModel _viewModel;

    public AssetListPage(AssetListViewModel viewModel)
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

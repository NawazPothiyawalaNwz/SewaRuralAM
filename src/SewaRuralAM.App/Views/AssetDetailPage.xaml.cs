using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class AssetDetailPage : ContentPage
{
    private readonly AssetDetailViewModel _viewModel;

    public AssetDetailPage(AssetDetailViewModel viewModel)
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

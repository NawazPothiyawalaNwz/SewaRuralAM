using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class MenuRightsPage : ContentPage
{
    private readonly MenuRightsViewModel _viewModel;

    public MenuRightsPage(MenuRightsViewModel viewModel)
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

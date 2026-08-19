using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class UserListPage : ContentPage
{
    private readonly UserListViewModel _viewModel;

    public UserListPage(UserListViewModel viewModel)
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

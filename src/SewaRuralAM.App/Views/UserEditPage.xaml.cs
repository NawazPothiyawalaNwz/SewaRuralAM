using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Views;

public partial class UserEditPage : ContentPage
{
    private readonly UserEditViewModel _viewModel;

    public UserEditPage(UserEditViewModel viewModel)
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

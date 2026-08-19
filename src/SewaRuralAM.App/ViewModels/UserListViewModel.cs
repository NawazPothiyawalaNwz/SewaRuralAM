using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class UserListViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastService _toastService;
    private readonly IMenuAccessService _menuAccessService;
    private List<User> _allUsers = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool canAdd;

    [ObservableProperty]
    private bool canEdit;

    public ObservableCollection<User> Users { get; } = new();

    public UserListViewModel(IUnitOfWork unitOfWork, IToastService toastService, IMenuAccessService menuAccessService)
    {
        _unitOfWork = unitOfWork;
        _toastService = toastService;
        _menuAccessService = menuAccessService;
        Title = "Users & Roles";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var rights = await _menuAccessService.GetRightsAsync("UserListPage");
            CanAdd = rights.CanAdd;
            CanEdit = rights.CanEdit;

            _allUsers = await _unitOfWork.Users.Query()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<User> query = _allUsers;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var text = SearchText.Trim();
            query = query.Where(u =>
                u.FullName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                u.UserName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        Users.Clear();
        foreach (var user in query)
            Users.Add(user);
    }

    [RelayCommand]
    private static async Task OpenUserAsync(User user)
    {
        if (user is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.UserEditPage)}?userId={user.Id}");
    }

    [RelayCommand]
    private async Task AddUserAsync()
    {
        if (!CanAdd)
        {
            _toastService.Show("You don't have permission to add users.", ToastKind.Error);
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(Views.UserEditPage)}?userId=0");
    }

    [RelayCommand]
    private async Task ToggleUserActiveAsync(User user)
    {
        if (user is null) return;

        if (!CanEdit)
        {
            _toastService.Show("You don't have permission to edit users.", ToastKind.Error);
            return;
        }

        user.IsActive = !user.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        _toastService.Show(user.IsActive ? $"{user.FullName} activated." : $"{user.FullName} deactivated.");
        await LoadAsync();
    }
}

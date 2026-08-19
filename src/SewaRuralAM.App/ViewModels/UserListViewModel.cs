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
    private List<User> _allUsers = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    public ObservableCollection<User> Users { get; } = new();

    public UserListViewModel(IUnitOfWork unitOfWork, IToastService toastService)
    {
        _unitOfWork = unitOfWork;
        _toastService = toastService;
        Title = "Users & Roles";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

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
    private static async Task AddUserAsync() =>
        await Shell.Current.GoToAsync($"{nameof(Views.UserEditPage)}?userId=0");

    [RelayCommand]
    private async Task ToggleUserActiveAsync(User user)
    {
        if (user is null) return;
        user.IsActive = !user.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        _toastService.Show(user.IsActive ? $"{user.FullName} activated." : $"{user.FullName} deactivated.");
        await LoadAsync();
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

[QueryProperty(nameof(UserId), "userId")]
public partial class UserEditViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastService _toastService;
    private readonly IMenuAccessService _menuAccessService;

    [ObservableProperty]
    private int userId;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private Role? selectedRole;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool canSave;

    public ObservableCollection<Role> Roles { get; } = new();

    public bool IsNewUser => UserId == 0;

    public UserEditViewModel(IUnitOfWork unitOfWork, IToastService toastService, IMenuAccessService menuAccessService)
    {
        _unitOfWork = unitOfWork;
        _toastService = toastService;
        _menuAccessService = menuAccessService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var rights = await _menuAccessService.GetRightsAsync("UserListPage");
        CanSave = IsNewUser ? rights.CanAdd : rights.CanEdit;

        Roles.Clear();
        foreach (var role in await _unitOfWork.Roles.GetAllAsync())
            Roles.Add(role);

        if (UserId > 0)
        {
            var user = await _unitOfWork.Users.Query()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == UserId);

            if (user is null) return;

            Title = "Edit User";
            UserName = user.UserName;
            FullName = user.FullName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            IsActive = user.IsActive;
            SelectedRole = Roles.FirstOrDefault(r => user.UserRoles.Any(ur => ur.RoleId == r.Id));
        }
        else
        {
            Title = "New User";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!CanSave)
        {
            ErrorMessage = "You don't have permission to save this user.";
            return;
        }

        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(FullName) || SelectedRole is null)
        {
            ErrorMessage = "User name, full name and role are required.";
            return;
        }

        if (IsNewUser && string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required for a new user.";
            return;
        }

        User user;
        var isNew = IsNewUser;

        if (isNew)
        {
            user = new User();
        }
        else
        {
            user = await _unitOfWork.Users.GetByIdAsync(UserId) ?? new User();
        }

        user.UserName = UserName.Trim();
        user.FullName = FullName.Trim();
        user.Email = Email.Trim();
        user.PhoneNumber = PhoneNumber.Trim();
        user.IsActive = IsActive;

        if (!string.IsNullOrWhiteSpace(Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);

        if (isNew)
            await _unitOfWork.Users.AddAsync(user);
        else
            _unitOfWork.Users.Update(user);

        await _unitOfWork.SaveChangesAsync();

        var existingUserRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == user.Id);
        var currentUserRole = existingUserRoles.FirstOrDefault();

        if (currentUserRole is null)
        {
            await _unitOfWork.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = SelectedRole.Id });
        }
        else if (currentUserRole.RoleId != SelectedRole.Id)
        {
            currentUserRole.RoleId = SelectedRole.Id;
            _unitOfWork.UserRoles.Update(currentUserRole);
        }

        await _unitOfWork.SaveChangesAsync();

        _toastService.Show(isNew ? $"User '{user.UserName}' created." : $"User '{user.UserName}' updated.", ToastKind.Success);
        await Shell.Current.GoToAsync("..");
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class MenuRightsViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private bool isRoleMode = true;

    [ObservableProperty]
    private Role? selectedRole;

    [ObservableProperty]
    private User? selectedUser;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ObservableCollection<Role> Roles { get; } = new();
    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<MenuRightRow> Rows { get; } = new();

    public MenuRightsViewModel(IUnitOfWork unitOfWork, IToastService toastService)
    {
        _unitOfWork = unitOfWork;
        _toastService = toastService;
        Title = "Menu Rights";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        Roles.Clear();
        foreach (var role in await _unitOfWork.Roles.GetAllAsync())
            Roles.Add(role);

        Users.Clear();
        foreach (var user in await _unitOfWork.Users.FindAsync(u => u.IsActive))
            Users.Add(user);

        SelectedRole ??= Roles.FirstOrDefault();
        SelectedUser ??= Users.FirstOrDefault();
        await LoadRowsAsync();
    }

    [RelayCommand]
    private async Task SetModeAsync(string mode)
    {
        IsRoleMode = mode == "role";
        await LoadRowsAsync();
    }

    partial void OnSelectedRoleChanged(Role? value)
    {
        if (IsRoleMode) _ = LoadRowsAsync();
    }

    partial void OnSelectedUserChanged(User? value)
    {
        if (!IsRoleMode) _ = LoadRowsAsync();
    }

    private async Task LoadRowsAsync()
    {
        var menus = await _unitOfWork.Menus.GetAllAsync();

        List<MenuRight> rights;
        if (IsRoleMode)
        {
            if (SelectedRole is null) return;
            rights = await _unitOfWork.MenuRights.FindAsync(r => r.RoleId == SelectedRole.Id && r.UserId == null);
        }
        else
        {
            if (SelectedUser is null) return;
            rights = await _unitOfWork.MenuRights.FindAsync(r => r.UserId == SelectedUser.Id);
        }

        Rows.Clear();
        foreach (var menu in menus.OrderBy(m => m.ParentMenuId).ThenBy(m => m.DisplayOrder))
        {
            var existing = rights.FirstOrDefault(r => r.MenuId == menu.Id);
            Rows.Add(new MenuRightRow(menu, existing));
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsRoleMode && SelectedRole is null) return;
        if (!IsRoleMode && SelectedUser is null) return;

        List<MenuRight> existingRights = IsRoleMode
            ? await _unitOfWork.MenuRights.FindAsync(r => r.RoleId == SelectedRole!.Id && r.UserId == null)
            : await _unitOfWork.MenuRights.FindAsync(r => r.UserId == SelectedUser!.Id);

        foreach (var row in Rows)
        {
            var right = existingRights.FirstOrDefault(r => r.MenuId == row.Menu.Id);
            var isNew = right is null;

            if (isNew)
            {
                right = IsRoleMode
                    ? new MenuRight { MenuId = row.Menu.Id, RoleId = SelectedRole!.Id }
                    : new MenuRight { MenuId = row.Menu.Id, UserId = SelectedUser!.Id };
            }

            right!.CanView = row.CanView;
            right.CanAdd = row.CanAdd;
            right.CanEdit = row.CanEdit;
            right.CanDelete = row.CanDelete;
            right.CanPrint = row.CanPrint;
            right.CanExport = row.CanExport;
            right.CanQrPrint = row.CanQrPrint;

            if (isNew)
                await _unitOfWork.MenuRights.AddAsync(right);
            else
                _unitOfWork.MenuRights.Update(right);
        }

        await _unitOfWork.SaveChangesAsync();
        StatusMessage = IsRoleMode
            ? $"Menu rights saved for role '{SelectedRole!.RoleName}'."
            : $"Menu rights saved for user '{SelectedUser!.UserName}' (overrides their role rights).";
        _toastService.Show(StatusMessage, ToastKind.Success);
    }
}

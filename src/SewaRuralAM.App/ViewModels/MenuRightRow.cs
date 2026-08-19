using CommunityToolkit.Mvvm.ComponentModel;
using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.App.ViewModels;

public partial class MenuRightRow : ObservableObject
{
    public Menu Menu { get; }

    [ObservableProperty]
    private bool canView;

    [ObservableProperty]
    private bool canAdd;

    [ObservableProperty]
    private bool canEdit;

    [ObservableProperty]
    private bool canDelete;

    [ObservableProperty]
    private bool canPrint;

    [ObservableProperty]
    private bool canExport;

    [ObservableProperty]
    private bool canQrPrint;

    public MenuRightRow(Menu menu, MenuRight? existing)
    {
        Menu = menu;
        if (existing is null) return;

        canView = existing.CanView;
        canAdd = existing.CanAdd;
        canEdit = existing.CanEdit;
        canDelete = existing.CanDelete;
        canPrint = existing.CanPrint;
        canExport = existing.CanExport;
        canQrPrint = existing.CanQrPrint;
    }
}

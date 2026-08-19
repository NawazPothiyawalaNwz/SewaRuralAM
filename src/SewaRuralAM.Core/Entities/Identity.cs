namespace SewaRuralAM.Core.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime? LastLoginDate { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role : BaseEntity
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<MenuRight> MenuRights { get; set; } = new List<MenuRight>();
}

public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public class Menu : BaseEntity
{
    public int? ParentMenuId { get; set; }
    public Menu? ParentMenu { get; set; }

    public string MenuName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<Menu> SubMenus { get; set; } = new List<Menu>();
    public ICollection<MenuRight> MenuRights { get; set; } = new List<MenuRight>();
}

public class MenuRight : BaseEntity
{
    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;

    public int? RoleId { get; set; }
    public Role? Role { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanPrint { get; set; }
    public bool CanExport { get; set; }
    public bool CanQrPrint { get; set; }
}

using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;
using SewaRuralAM.Infrastructure.Data;

namespace SewaRuralAM.Infrastructure.Services;

// User-level MenuRights (UserId set) override Role-level MenuRights (RoleId set) when both exist.
public class MenuRightsService : IMenuRightsService
{
    private readonly AppDbContext _context;

    public MenuRightsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Menu>> GetAuthorizedMenusAsync(int userId)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var allMenus = await _context.Menus
            .Where(m => m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        var rights = await _context.MenuRights
            .Where(mr => mr.IsActive && (mr.UserId == userId || (mr.RoleId != null && roleIds.Contains(mr.RoleId.Value))))
            .ToListAsync();

        var visibleMenuIds = new HashSet<int>();
        foreach (var menu in allMenus)
        {
            var userRight = rights.FirstOrDefault(r => r.MenuId == menu.Id && r.UserId == userId);
            var roleRight = rights.Where(r => r.MenuId == menu.Id && r.RoleId != null).OrderByDescending(r => r.CanView).FirstOrDefault();
            var effective = userRight ?? roleRight;

            if (effective?.CanView == true)
                visibleMenuIds.Add(menu.Id);
        }

        // A parent menu stays visible if any of its submenus are visible, even without its own explicit right.
        foreach (var menu in allMenus.Where(m => m.ParentMenuId != null))
        {
            if (visibleMenuIds.Contains(menu.Id) && menu.ParentMenuId.HasValue)
                visibleMenuIds.Add(menu.ParentMenuId.Value);
        }

        return allMenus.Where(m => visibleMenuIds.Contains(m.Id)).ToList();
    }

    public async Task<MenuRight?> GetEffectiveRightsAsync(int userId, int menuId)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var userRight = await _context.MenuRights
            .FirstOrDefaultAsync(r => r.MenuId == menuId && r.UserId == userId && r.IsActive);
        if (userRight is not null)
            return userRight;

        return await _context.MenuRights
            .Where(r => r.MenuId == menuId && r.RoleId != null && roleIds.Contains(r.RoleId.Value) && r.IsActive)
            .OrderByDescending(r => r.CanEdit)
            .FirstOrDefaultAsync();
    }

    public async Task<MenuRight?> GetEffectiveRightsForRouteAsync(int userId, string route)
    {
        var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Route == route && m.IsActive);
        return menu is null ? null : await GetEffectiveRightsAsync(userId, menu.Id);
    }
}

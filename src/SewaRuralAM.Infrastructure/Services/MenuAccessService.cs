using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.Infrastructure.Services;

public class MenuAccessService : IMenuAccessService
{
    private readonly IMenuRightsService _menuRightsService;
    private readonly IAuthService _authService;

    public MenuAccessService(IMenuRightsService menuRightsService, IAuthService authService)
    {
        _menuRightsService = menuRightsService;
        _authService = authService;
    }

    public async Task<MenuRight> GetRightsAsync(string route)
    {
        var userId = _authService.CurrentUser?.Id;
        if (userId is null) return new MenuRight();

        var rights = await _menuRightsService.GetEffectiveRightsForRouteAsync(userId.Value, route);
        return rights ?? new MenuRight();
    }
}

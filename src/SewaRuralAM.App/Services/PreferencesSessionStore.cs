using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.Services;

public class PreferencesSessionStore : ISessionStore
{
    private const string UserIdKey = "CurrentUserId";

    public void SaveUserId(int userId) => Preferences.Default.Set(UserIdKey, userId);

    public int? GetUserId()
    {
        var id = Preferences.Default.Get(UserIdKey, -1);
        return id > 0 ? id : null;
    }

    public void Clear() => Preferences.Default.Remove(UserIdKey);
}

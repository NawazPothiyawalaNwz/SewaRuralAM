using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.Tests;

public class FakeSessionStore : ISessionStore
{
    private int? _userId;

    public void SaveUserId(int userId) => _userId = userId;

    public int? GetUserId() => _userId;

    public void Clear() => _userId = null;
}

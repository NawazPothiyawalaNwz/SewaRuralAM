using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Infrastructure.Data;

namespace SewaRuralAM.Tests;

// Wraps a single open in-memory SQLite connection so every AppDbContext created from it
// shares the same database (a plain ":memory:" connection string would give each context its own).
public sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

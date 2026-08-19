using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;
using SewaRuralAM.Infrastructure.Data;

namespace SewaRuralAM.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

    public async Task<List<T>> GetAllAsync() => await DbSet.AsNoTracking().ToListAsync();

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.FirstOrDefaultAsync(predicate);

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public void Update(T entity)
    {
        entity.ModifiedDate = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public void Remove(T entity) => DbSet.Remove(entity);

    public IQueryable<T> Query() => DbSet.AsQueryable();
}

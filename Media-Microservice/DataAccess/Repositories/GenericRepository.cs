using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    // Use DbContext instead AppDbContext to reduce dependent
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    //=============================================
    public bool HasChanges(T newEntity, T trackedEntity) =>
        typeof(T).GetProperties().Any(prop => !Equals(prop.GetValue(trackedEntity), prop.GetValue(newEntity)));
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression) => await _dbSet.AsNoTracking().AnyAsync(expression);
    public async Task<int> CountAsync(Expression<Func<T, bool>> expression) => await _dbSet.AsNoTracking().CountAsync(expression);

    public async Task<List<T>?> GetListAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool hasTracking = false,
        bool asSplitQuery = false)
    {
        return await BuildQuery(predicate, include, hasTracking, asSplitQuery).ToListAsync();
    }

    public async Task<List<TResult>?> GetListAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool hasTracking = false,
        bool asSplitQuery = false)
    {
        var query = BuildQuery(predicate, include, hasTracking, asSplitQuery);
        return await query.Select(selector).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetPagedAsync(
        int pageNumber, int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asSplitQuery = false)
    {
        var query = BuildQuery(predicate, include, hasTracking: false, asSplitQuery);

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<T?> GetOneAsync(
        Expression<Func<T, bool>> expression,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool hasTracking = true, bool asSplitQuery = false)
    => await BuildQuery(expression, include, hasTracking, asSplitQuery).FirstOrDefaultAsync();

    public async Task<T?> GetByIdAsync<Tkey>(Tkey id) => await _dbSet.FindAsync(id);

    public Task AddAsync(T entity) { _dbSet.Add(entity); return Task.CompletedTask; }
    public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

    public Task UpdateAsync(T entity) { _dbSet.Update(entity); return Task.CompletedTask; }
    public Task DeleteAsync(T entity) { _dbSet.Remove(entity); return Task.CompletedTask; }
    public Task DeleteRangeAsync(IEnumerable<T> entities) { _dbSet.RemoveRange(entities); return Task.CompletedTask; }

    // -------------------< HELPER METHODS >-------------------
    private IQueryable<T> BuildQuery(
        Expression<Func<T, bool>>? predicate,
        Func<IQueryable<T>, IQueryable<T>>? include,
        bool hasTracking,
        bool useSplitQuery = false)
    {
        IQueryable<T> query = _dbSet;
        if (!hasTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        if (predicate != null) query = query.Where(predicate);
        if (useSplitQuery) query = query.AsSplitQuery();
        return query;
    }
}

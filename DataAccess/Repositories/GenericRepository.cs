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

    /// <param name="expression">Filter Condition (eg: x => x.Status == 1)</param>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression) => await _dbSet.AnyAsync(expression);

    /// <param name="expression">Filter Condition (eg: x => x.Status == 1)</param>
    public async Task<int> CountAsync(Expression<Func<T, bool>> expression) => await _dbSet.CountAsync(expression);

    /// <summary>
    /// Get list entity without select columns (model's properties).
    /// </summary>
    public async Task<List<T>?> GetListAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool hasTracking = false,
        bool useSplitQuery = false)
    {
        return await BuildQuery(predicate, include, hasTracking, useSplitQuery).ToListAsync();
    }

    /// <summary>
    /// Get list entity with flexible option.
    /// </summary>
    /// <param name="predicate">Filter Condition (eg: x => x.Status == 1)</param>
    /// <param name="include">Query to join table (eg: q => q.Include(x => x.Category).ThenInclude(cate => cate.Parent))</param>
    /// <param name="selector">Choose specific properties (eg: x => new { x.Id, x.Name }). Leave null if want to get all</param>
    /// <param name="hasTracking">Set "true" if want to Update,Delete after query. | "false" to increase performance if data Read-Only.</param>
    /// <param name="useSplitQuery">Set "true" to increase performance when use Include more than 2 child-tables (1-N).</param>
    public async Task<List<TResult>?> GetListAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool hasTracking = false,
        bool useSplitQuery = false)
    {
        var query = BuildQuery(predicate, include, hasTracking, useSplitQuery);
        return await query.Select(selector).ToListAsync();
    }

    /// <summary>
    /// Lấy dữ liệu phân trang.
    /// </summary>
    /// <param name="pageNumber">Current page (start from 1)</param>
    /// <param name="pageSize">Number of records in a page</param>
    /// <param name="predicate">Filter Condition (eg: x => x.Status == 1)</param>
    /// <param name="orderBy">Sort by (VD: q => q.OrderByDescending(x => x.CreatedDate))</param>
    /// <param name="include">Query to join table (eg: q => q.Include(x => x.Category).ThenInclude(cate => cate.Parent))</param>
    public async Task<IEnumerable<T>> GetPagedAsync(
        int pageNumber, int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        var query = BuildQuery(predicate, include, hasTracking: false);

        if (orderBy != null) query = orderBy(query);

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    /// <param name="expression">Filter Condition (eg: x => x.Status == 1)</param>
    /// <param name="include">Query to join table (eg: q => q.Include(x => x.Category).ThenInclude(cate => cate.Parent))</param>
    /// <param name="hasTracking">Set "true" if want to Update,Delete after query. | "false" to increase performance if data Read-Only.</param>
    /// <returns></returns>
    public async Task<T?> GetOneAsync(
        Expression<Func<T, bool>> expression,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool hasTracking = true)
    => await BuildQuery(expression, include, hasTracking).FirstOrDefaultAsync();

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

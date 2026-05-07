using System.Linq.Expressions;

namespace DataAccess.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    /// <param name="expression">Filter Condition (eg: x => x.Status == 1)</param>
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    /// <param name="expression">Filter Condition (eg: x => x.Status == 1)</param>
    Task<int> CountAsync(Expression<Func<T, bool>> expression);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<T?> GetByIdAsync<Tkey>(Tkey id);

    /// <summary>
    ///     Get list entity without select columns (model's properties).
    /// </summary>
    /// <param name="predicate">Filter Condition (eg: x => x.Status == 1)</param>
    /// <param name="include">Query to join table (eg: q => q.Include(x => x.Category).ThenInclude(cate => cate.Parent))</param>
    /// <param name="hasTracking">Set "true" if want to Update,Delete after query. | "false" to increase performance if data Read-Only.</param>
    /// <param name="useSplitQuery">Set "true" to increase performance when use Include more than 2 child-tables (1-N).</param>
    Task<List<T>?> GetListAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null, bool hasTracking = false, bool useSplitQuery = false);

    /// <summary>
    ///     Get list entity with flexible option.
    /// </summary>
    /// <param name="predicate">Filter Condition (eg: x => x.Status == 1)</param>
    /// <param name="include">Query to join table (eg: q => q.Include(x => x.Category).ThenInclude(cate => cate.Parent))</param>
    /// <param name="selector">Choose specific properties (eg: x => new { x.Id, x.Name }). Leave null if want to get all</param>
    /// <param name="hasTracking">Set "true" if want to Update,Delete after query. | "false" to increase performance if data Read-Only.</param>
    /// <param name="useSplitQuery">Set "true" to increase performance when use Include more than 2 child-tables (1-N).</param>
    Task<List<TResult>?> GetListAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, Func<IQueryable<T>, IQueryable<T>>? include = null, bool hasTracking = false, bool useSplitQuery = false);
    
    /// <summary>
    ///     To query and get a single record from database.
    /// </summary>
    /// <param name="expression">Filter Condition (eg: x => x.Status == 1)</param>
    /// <param name="include">Query to join table (eg: q => q.Include(x => x.Category).ThenInclude(cate => cate.Parent))</param>
    /// <param name="hasTracking">Set "true" if want to Update,Delete after query. | "false" to increase performance if data Read-Only.</param>
    /// <returns>A model with properties contained data</returns>
    Task<T?> GetOneAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>? include = null, bool hasTracking = true);

    /// <summary>
    ///     Get paged list data.
    /// </summary>
    /// <param name="pageNumber">Current page (start from 1)</param>
    /// <param name="pageSize">Number of records in a page</param>
    /// <param name="predicate">Filter Condition (eg: x => x.Status == 1)</param>
    /// <param name="orderBy">Sort by (VD: q => q.OrderByDescending(x => x.CreatedDate))</param>
    /// <param name="include">Query to join table (eg: q => q.Include(x => x.Category).ThenInclude(cate => cate.Parent))</param>
    /// <returns>IEnumerable: a kind of list that can read-only</returns>
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>? include = null);
    bool HasChanges(T newEntity, T trackedEntity);
    Task UpdateAsync(T entity);
}
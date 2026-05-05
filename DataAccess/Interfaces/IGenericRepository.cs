using System.Linq.Expressions;

namespace DataAccess.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    Task<int> CountAsync(Expression<Func<T, bool>> expression);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<T?> GetByIdAsync<Tkey>(Tkey id);
    Task<List<T>?> GetListAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null, bool hasTracking = false, bool useSplitQuery = false);
    Task<List<TResult>?> GetListAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, Func<IQueryable<T>, IQueryable<T>>? include = null, bool hasTracking = false, bool useSplitQuery = false);
    Task<T?> GetOneAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>? include = null, bool hasTracking = true);
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>? include = null);
    bool HasChanges(T newEntity, T trackedEntity);
    Task UpdateAsync(T entity);
}
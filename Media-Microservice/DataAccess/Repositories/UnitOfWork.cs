using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;

namespace DataAccess.Repositories;

public class UnitOfWork(AVEDbContext context) : IUnitOfWork
{
    private readonly DbContext _context = context;
    private IDbContextTransaction? _transaction;
    private Hashtable? _repositories; // Save (cache) GenericRepo called

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        _repositories ??= [];

        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);
            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<TEntity>)_repositories[type]!;
    }

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();


    // ---------------< TRANSACTION >---------------
    public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();
    public async Task CommitAsync()
    {
        try
        {
            await CompleteAsync(); // Save before commit
            if (_transaction != null) await _transaction.CommitAsync();
        }
        finally
        {
            if (_transaction != null) await _transaction.DisposeAsync();
        }
    }
    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary><![CDATA[
    ///public async Task TransferMoneyAsync(int fromAccountId, int toAccountId, decimal amount)
    ///{
    ///    // Create Transaction
    ///    await _unitOfWork.BeginTransactionAsync();
    ///
    ///    try
    ///    {
    ///        // 1. decrease money
    ///        ...
    ///        await accountRepo.UpdateAsync(fromAcc);
    ///
    ///        // 2. add money
    ///        ...
    ///        await accountRepo.UpdateAsync(toAcc);
    ///
    ///        // 3. write log
    ///        await logRepo.AddAsync(new TransactionHistory { info...});
    ///
    ///        // 4. Confirm data save success
    ///        await _unitOfWork.CommitAsync();
    ///    }
    ///    catch (Exception ex)
    ///    {
    ///        // if error (eg: not enough money, server error), ROLLBACK all
    ///        await _unitOfWork.RollbackAsync();
    ///        throw new Exception("Transfer failed", ex);
    ///    }
    ///}
    /// ]]></summary>
    private static void ExampleTransaction() { }
}

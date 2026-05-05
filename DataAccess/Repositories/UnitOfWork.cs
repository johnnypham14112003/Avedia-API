using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;

namespace DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction? _transaction;
    private Hashtable? _repositories; // Lưu trữ (cache) các GenericRepo đã được gọi

    // Backing field cho Custom Repo
    //private IExampleCustomRepository? _exampleRepository;
    public UnitOfWork(AVEDbContext context)
    {
        _context = context;
    }

    // Lazy load: if not call -> not create new AccountRepository()
    //public IExampleCustomRepository ExampleModels => _exampleRepository ??= new ExampleCustomRepository(_context);

    // This method auto create and return GenericRepository for any Entity
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        _repositories ??= new Hashtable();

        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);
            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<TEntity>)_repositories[type]!;
    }

    /// <summary>
    /// This method is "Save Change" of DbContext for saving data that previous handle
    /// </summary>
    /// <returns>Number of affected records</returns>
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
}

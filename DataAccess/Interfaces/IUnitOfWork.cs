namespace DataAccess.Interfaces;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    ///     This method auto create and return GenericRepository for any Entity.<para/>
    ///     <![CDATA[Example:
    ///         var accountRepo = _unitOfWork.Repository<Account>();
    ///         var fromAcc = await accountRepo.GetByIdAsync(fromAccountId);
    ///     ]]>
    /// </summary>
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    ///     This method is same <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(CancellationToken)()">Save Change</see> of <see cref="Microsoft.EntityFrameworkCore.DbContext">DbContext</see> for saving data that previous handle
    /// </summary>
    /// <returns>Number of affected records</returns>
    Task<int> CompleteAsync();

    /// <summary>
    ///     One of three methods for conducting secure transactions.<para/>
    ///     <![CDATA[Including:
    ///     + ]]><see cref="Repositories.UnitOfWork.BeginTransactionAsync">BeginTransactionAsync( )</see><![CDATA[.
    ///     + ]]><see cref="Repositories.UnitOfWork.CommitAsync">CommitAsync( )</see><![CDATA[.
    ///     + ]]><see cref="Repositories.UnitOfWork.RollbackAsync">RollbackAsync( )</see><para/>
    ///     Example: <see cref="Repositories.UnitOfWork.ExampleTransaction">ExampleTransaction</see>
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    ///     One of three methods for conducting secure transactions.<para/>
    ///     <![CDATA[Including:
    ///     + ]]><see cref="Repositories.UnitOfWork.BeginTransactionAsync">BeginTransactionAsync( )</see><![CDATA[.
    ///     + ]]><see cref="Repositories.UnitOfWork.CommitAsync">CommitAsync( )</see><![CDATA[.
    ///     + ]]><see cref="Repositories.UnitOfWork.RollbackAsync">RollbackAsync( )</see><para/>
    ///     Example: <see cref="Repositories.UnitOfWork.ExampleTransaction">ExampleTransaction</see>
    /// </summary>
    Task CommitAsync();

    /// <summary>
    ///     One of three methods for conducting secure transactions.<para/>
    ///     <![CDATA[Including:
    ///     + ]]><see cref="Repositories.UnitOfWork.BeginTransactionAsync">BeginTransactionAsync( )</see><![CDATA[.
    ///     + ]]><see cref="Repositories.UnitOfWork.CommitAsync">CommitAsync( )</see><![CDATA[.
    ///     + ]]><see cref="Repositories.UnitOfWork.RollbackAsync">RollbackAsync( )</see><para/>
    ///     Example: <see cref="Repositories.UnitOfWork.ExampleTransaction">ExampleTransaction</see>
    /// </summary>
    Task RollbackAsync();
}
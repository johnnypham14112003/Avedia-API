namespace DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // This method auto create repository
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

        // For custom repository with custom methods
        // IExampleCustomRepository Example {get;}

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task<int> CompleteAsync();
        Task RollbackAsync();
    }
}
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;

namespace BusinessLogic.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    /* Example
     public async Task TransferMoneyAsync(int fromAccountId, int toAccountId, decimal amount)
    {
        // Create Transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // AUTO call Repo without creating class AccountRepository
            var accountRepo = _unitOfWork.Repository<Account>(); 
            var logRepo = _unitOfWork.Repository<TransactionHistory>();

            // 1. decrease money
            var fromAcc = await accountRepo.GetByIdAsync(fromAccountId);
            fromAcc.Balance -= amount;
            await accountRepo.UpdateAsync(fromAcc);

            // 2. add money
            var toAcc = await accountRepo.GetByIdAsync(toAccountId);
            toAcc.Balance += amount;
            await accountRepo.UpdateAsync(toAcc);

            // 3. write log
            await logRepo.AddAsync(new TransactionHistory { Amount = amount, Date = DateTime.Now });

            // 4. Confirm data save success
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            // if error (eg: not enough money, server error), ROLLBACK all
            await _unitOfWork.RollbackAsync();
            throw new Exception("Transfer failed", ex);
        }
    }
    
    public async Task GetComplexData()
    {
        var repo = _unitOfWork.Repository<Account>();
        var result = await repo.GetListAsync(
            predicate: x => x.Status == "Active",
            include: q => q.Include(x => x.Profile).ThenInclude(p => p.Address),
            hasTracking: false
        );
    }
    */
}

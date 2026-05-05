using DataAccess.Base;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;
/*
public class ExampleCustomRepository : GenericRepository<ExampleModel>, IExampleCustomRepository
{
    public ExampleCustomRepository(DbContext context) : base(context)
    {
    }

    public async Task<List<ExampleModel>> GetTopSpendersAsync(int topCount)
    {
        // Use _dbSet declared in GenericRepository
        return await _dbSet
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.TotalSpent)
            .Take(topCount)
            .ToListAsync();
    }
}
*/
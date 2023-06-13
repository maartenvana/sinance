using Microsoft.EntityFrameworkCore;
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<SinanceContext> dbContextFactory;

    public TransactionService(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<List<TransactionModel>> SearchTransactionsPaged(SearchTransactionsFilterModel filter)
    {
        using var context = this.dbContextFactory.CreateDbContext();

        var query = context.Transactions
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(x => x.Description.ToLower().Contains(filter.Description.ToLower()));

        if (filter.BankAccountId != null)
            query = query.Where(x => x.BankAccountId == filter.BankAccountId);

        if (filter.Category != null)
        {
            if (filter.Category == -1)
            {
                query = query.Where(x => x.CategoryId == null);
            }
            else
            {
                query = query.Where(x => x.CategoryId == filter.Category);
            }
        }

        var transactionEntities = await query
            .OrderByDescending(x => x.Date)
            .Skip(filter.Page * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return transactionEntities.ToDto().ToList();
    }
}

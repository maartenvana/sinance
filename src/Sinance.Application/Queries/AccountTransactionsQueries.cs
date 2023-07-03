using Microsoft.EntityFrameworkCore;
using Sinance.Contracts.Transaction;
using Sinance.Domain.Model;
using Sinance.Infrastructure;

namespace Sinance.Application.Queries
{
    public class AccountTransactionQueries : IAccountTransactionQueries
    {
        private readonly SinanceContext context;

        public AccountTransactionQueries(SinanceContext context)
        {
            this.context = context;
        }

        public async Task<List<AccountTransaction>> FindTransactionsAsync(FindAccountTransactionsFilter filter)
        {
            var query = context.Transactions
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Description))
                query = query.Where(x => x.Description.ToLower().Contains(filter.Description.ToLower()));

            if (filter.BankAccountId != null)
                query = query.Where(x => x.BankAccountId == filter.BankAccountId);

            if (filter.CategoryId != null)
                query = query.Where(x => x.CategoryId == filter.CategoryId);

            var transactionEntities = await query
                .AsNoTracking()
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.Name)
                .Skip(filter.Page * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return transactionEntities.ToList();
        }
    }
}

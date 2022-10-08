using Microsoft.EntityFrameworkCore;
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.BankAccount;
using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.BlazorApp.Storage;
using Sinance.Storage;
using Sinance.Storage.Entities;

namespace Sinance.BlazorApp.Business.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISinanceDbContextFactory<SinanceContext> dbContextFactory;

        public TransactionService(ISinanceDbContextFactory<SinanceContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<List<TransactionModel>> SearchTransactionsPagedAsync(SearchTransactionsFilterModel filter)
        {
            using var context = dbContextFactory.CreateDbContext();

            var query = context.Transactions
                .Include(x => x.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Description))
                query = query.Where(x => x.Description.ToLower().Contains(filter.Description.ToLower()));

            if (filter.BankAccountId != BankAccountModel.All.Id)
                query = query.Where(x => x.BankAccountId == filter.BankAccountId);

            if (filter.CategoryId != CategoryModel.All.Id)
                query = query.Where(x => x.CategoryId == filter.CategoryId);

            var transactionEntities = await query
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.Name)
                .Skip(filter.Page * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return transactionEntities.ToDto().ToList();
        }
    }
}

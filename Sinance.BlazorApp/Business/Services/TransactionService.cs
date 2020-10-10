using Microsoft.EntityFrameworkCore;
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.BlazorApp.Storage;
using Sinance.Communication.Model.StandardReport.Yearly;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Services
{
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

            if (filter.BankAccountId != null)
                query = query.Where(x => x.BankAccountId == filter.BankAccountId);

            if (filter.NoCategory == true)
                query = query.Where(x => x.Category == null);

            if (filter.Categories.Any())
                query = query.Where(x => filter.Categories.Any(y => y == x.Category.Id));

            var transactionEntities = await query.Skip(filter.Page * filter.PageSize).Take(filter.PageSize).ToListAsync();

            return transactionEntities.ToDto().ToList();
        }
    }
}

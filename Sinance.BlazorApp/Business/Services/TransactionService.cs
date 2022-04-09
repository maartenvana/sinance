using Microsoft.EntityFrameworkCore;
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.BankAccount;
using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.BlazorApp.Extensions;
using Sinance.BlazorApp.Storage;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISinanceDbContextFactory<SinanceContext> dbContextFactory;
        private readonly IUserIdProvider userIdProvider;
        private readonly IBankAccountService bankAccountService;

        public TransactionService(
            ISinanceDbContextFactory<SinanceContext> dbContextFactory,
            IUserIdProvider userIdProvider,
            IBankAccountService bankAccountService) // TODO: VIOLATION BUT EVERYTHING SHOULD GO TO HANDLERS
        {
            this.dbContextFactory = dbContextFactory;
            this.userIdProvider = userIdProvider;
            this.bankAccountService = bankAccountService;
        }

        public async Task<List<TransactionModel>> SplitTransactionAsync(SplitTransactionModel splitModel)
        {
            using var context = dbContextFactory.CreateDbContext();

            var transactionToSplit = context.Transactions.Single(x => x.Id == splitModel.SourceTransactionId);

            var newTransactionEntities = transactionToSplit.SplitToNewTransactions(splitModel.NewTransactions);

            if(newTransactionEntities.Sum(x => x.Amount) != transactionToSplit.Amount)
                throw new ArgumentOutOfRangeException(paramName: nameof(splitModel), message: "New transaction sum is not equal to source transaction");

            context.Transactions.AddRange(newTransactionEntities);
            context.Transactions.Remove(transactionToSplit);

            await context.SaveChangesAsync();

            return newTransactionEntities.ToDto().ToList();
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

        public async Task<TransactionModel> UpsertTransactionAsync(UpsertTransactionModel upsertTransactionModel)
        {
            using var context = dbContextFactory.CreateDbContext();

            TransactionEntity transactionEntity;
            if (upsertTransactionModel.IsNew)
            {
                transactionEntity = upsertTransactionModel.ToNewTransactionEntity(userIdProvider.GetCurrentUserId());
                context.Transactions.Add(transactionEntity);
            }
            else
            {
                transactionEntity = context.Transactions.Single(x => x.Id == upsertTransactionModel.Id);
                transactionEntity.UpdateFromUpsertModel(upsertTransactionModel);
            }

            await context.SaveChangesAsync();

            await bankAccountService.RecalculateBalanceAsync(transactionEntity.BankAccountId);

            return transactionEntity.ToDto();
        }

        public async Task DeleteTransactionAsync(TransactionModel transaction)
        {
            using var context = dbContextFactory.CreateDbContext();

            var transactionToRemove = context.Transactions.Single(x => x.Id == transaction.Id);

            context.Remove(transactionToRemove);

            await context.SaveChangesAsync();
        }
    }
}

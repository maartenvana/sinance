using Microsoft.EntityFrameworkCore;
using Sinance.Business.Calculations;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Transactions;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;
    private readonly IUserIdProvider _userIdProvider;

    public TransactionService(
        IDbContextFactory<SinanceContext> dbContextFactory,
        IUserIdProvider userIdProvider)
    {
        _dbContextFactory = dbContextFactory;
        _userIdProvider = userIdProvider;
    }

    public async Task<TransactionModel> ClearTransactionCategoriesForCurrentUser(int transactionId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var transaction = await context.Transactions
            .SingleOrDefaultAsync(item => item.Id == transactionId);

        if (transaction == null)
            throw new NotFoundException(nameof(TransactionEntity));

        transaction.CategoryId = null;
        transaction.Category = null;

        await context.SaveChangesAsync();

        return transaction.ToDto();
    }

    public async Task<TransactionModel> CreateTransactionForCurrentUser(TransactionModel transactionModel)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccount = await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == transactionModel.BankAccountId);

        if (bankAccount == null)
            throw new NotFoundException(nameof(BankAccountEntity));

        var transactionEntity = transactionModel.ToNewEntity(_userIdProvider.GetCurrentUserId());
        await context.Transactions.AddAsync(transactionEntity);
        await context.SaveChangesAsync();

        // First save the transaction, then recalculate the balance.
        bankAccount.CurrentBalance = await BankAccountCalculations.CalculateCurrentBalanceForBankAccount(context, bankAccount);
        await context.SaveChangesAsync();

        return transactionEntity.ToDto();
    }

    public async Task DeleteTransactionForCurrentUser(int transactionId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var transaction = await context.Transactions
            .Include(x => x.BankAccount)
            .SingleOrDefaultAsync(item => item.Id == transactionId);

        if (transaction == null)
            throw new NotFoundException(nameof(TransactionEntity));

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();

        transaction.BankAccount.CurrentBalance = await BankAccountCalculations.CalculateCurrentBalanceForBankAccount(context, transaction.BankAccount);

        await context.SaveChangesAsync();
    }

    public async Task<List<TransactionModel>> GetBiggestExpensesForYearForCurrentUser(int year, int count, int skip, params int?[] excludeCategoryIds)
    {
        using var context = _dbContextFactory.CreateDbContext();

        excludeCategoryIds ??= System.Array.Empty<int?>();

        var transactions = await context.Transactions
            .Where(x => !excludeCategoryIds.Contains(x.CategoryId) && x.Date.Year == year)
            .OrderBy(x => x.Amount)
            .Skip(skip)
            .Take(count)
            .Include(x => x.Category)
            .ToListAsync();

        return transactions.ToDto().ToList();
    }

    public async Task<TransactionModel> GetTransactionByIdForCurrentUser(int transactionId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var transaction = await FindTransaction(transactionId, context);

        if (transaction == null)
            throw new NotFoundException(nameof(TransactionEntity));

        return transaction.ToDto();
    }

    public async Task<List<TransactionModel>> GetTransactionsForBankAccountForCurrentUser(int bankAccountId, int count, int skip)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var transactions = await context.Transactions
            .Where(x => x.BankAccountId == bankAccountId)
            .OrderByDescending(x => x.Date)
            .Skip(skip)
            .Take(count)
            .Include(x => x.Category)
            .ToListAsync();

        return transactions.ToDto().ToList();
    }

    public async Task<List<TransactionModel>> GetTransactionsForMonthForCurrentUser(int year, int month)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var transactions = await context.Transactions
            .Where(x => x.Date.Year == year && x.Date.Month == month)
            .Include(x => x.Category)
            .ToListAsync();

        return transactions.ToDto().ToList();
    }

    public async Task<TransactionModel> OverwriteTransactionCategoriesForCurrentUser(int transactionId, int categoryId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var transaction = await FindTransaction(transactionId, context);

        if (transaction == null)
            throw new NotFoundException(nameof(TransactionEntity));

        var category = await context.Categories.SingleOrDefaultAsync(item => item.Id == categoryId);

        if (category == null)
            throw new NotFoundException(nameof(CategoryEntity));

        transaction.CategoryId = category.Id;

        await context.SaveChangesAsync();

        transaction = await FindTransaction(transactionId, context);

        return transaction.ToDto();
    }

    public async Task<TransactionModel> UpdateTransactionForCurrentUser(TransactionModel transactionModel)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var existingTransaction = await FindTransaction(transactionModel.Id, context);

        if (existingTransaction == null)
            throw new NotFoundException(nameof(TransactionEntity));

        existingTransaction.UpdateFromModel(transactionModel);

        await context.SaveChangesAsync();

        existingTransaction.BankAccount.CurrentBalance = await BankAccountCalculations.CalculateCurrentBalanceForBankAccount(context, existingTransaction.BankAccount);
        await context.SaveChangesAsync();

        return existingTransaction.ToDto();
    }

    private static async Task<TransactionEntity> FindTransaction(int transactionId, SinanceContext context)
    {
        return await context.Transactions
            .Include(x => x.BankAccount)
            .Include(x => x.Category)
            .SingleAsync(item => item.Id == transactionId);
    }
}
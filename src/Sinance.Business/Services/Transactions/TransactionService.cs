using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Transactions;

public class TransactionService : ITransactionService
{
    private readonly IBankAccountCalculationService _bankAccountCalculationService;
    private readonly Func<IUnitOfWork> _unitOfWork;
    private readonly IUserIdProvider _userIdProvider;

    public TransactionService(
        Func<IUnitOfWork> unitOfWork,
        IBankAccountCalculationService bankAccountCalculationService,
        IUserIdProvider userIdProvider)
    {
        _unitOfWork = unitOfWork;
        _bankAccountCalculationService = bankAccountCalculationService;
        _userIdProvider = userIdProvider;
    }

    public async Task<TransactionModel> ClearTransactionCategoriesForCurrentUser(int transactionId)
    {
        using var unitOfWork = _unitOfWork();

        var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item => item.Id == transactionId);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        transaction.CategoryId = null;

        await unitOfWork.SaveAsync();

        return transaction.ToDto();
    }

    public async Task<TransactionModel> CreateTransactionForCurrentUser(TransactionModel transactionModel)
    {
        using var unitOfWork = _unitOfWork();

        var bankAccount = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == transactionModel.BankAccountId);

        if (bankAccount == null)
        {
            throw new NotFoundException(nameof(BankAccountEntity));
        }

        var transactionEntity = transactionModel.ToNewEntity(_userIdProvider.GetCurrentUserId());
        unitOfWork.TransactionRepository.Insert(transactionEntity);
        await unitOfWork.SaveAsync();

        // First save the transaction, then recalculate the balance.
        bankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(unitOfWork, bankAccount);
        await unitOfWork.SaveAsync();

        return transactionEntity.ToDto();
    }

    public async Task DeleteTransactionForCurrentUser(int transactionId)
    {
        using var unitOfWork = _unitOfWork();
        var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item =>
            item.Id == transactionId,
            includeProperties: new string[] {
                nameof(TransactionEntity.BankAccount)
            });

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        unitOfWork.TransactionRepository.Delete(transaction);
        await unitOfWork.SaveAsync();

        transaction.BankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(unitOfWork, transaction.BankAccount);

        await unitOfWork.SaveAsync();
    }

    public async Task<List<TransactionModel>> GetBiggestExpensesForYearForCurrentUser(int year, int count, int skip, params int?[] excludeCategoryIds)
    {
        using var unitOfWork = _unitOfWork();

        excludeCategoryIds ??= Array.Empty<int?>();

        var transactions = await unitOfWork.TransactionRepository.FindTopAscending(findQuery: transaction =>
                    !excludeCategoryIds.Any(y => y == transaction.CategoryId) &&
                    transaction.Date.Year == year,
                    orderByAscending: x => x.Amount,
                    count: count,
                    skip: skip,
                    includeProperties: new string[] { nameof(TransactionEntity.Category) });

        return transactions.ToDto().ToList();
    }

    public async Task<TransactionModel> GetTransactionByIdForCurrentUser(int transactionId)
    {
        using var unitOfWork = _unitOfWork();

        var transaction = await FindTransaction(transactionId, unitOfWork);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        return transaction.ToDto();
    }

    public async Task<List<TransactionModel>> GetTransactionsForBankAccountForCurrentUser(int bankAccountId, int count, int skip)
    {
        using var unitOfWork = _unitOfWork();

        var transactions = await unitOfWork.TransactionRepository
            .FindTopDescending(item => item.BankAccountId == bankAccountId,
                                orderByDescending: x => x.Date,
                                count: count,
                                skip: skip,
                                includeProperties: new string[] { nameof(TransactionEntity.Category) });

        return transactions.ToDto().ToList();
    }

    public async Task<List<TransactionModel>> GetTransactionsForMonthForCurrentUser(int year, int month)
    {
        using var unitOfWork = _unitOfWork();

        var transactions = await unitOfWork.TransactionRepository.FindAll(findQuery: x =>
                    x.Date.Year == year &&
                    x.Date.Month == month,
                    includeProperties: new string[] { nameof(TransactionEntity.Category) });

        return transactions.ToDto().ToList();
    }

    public async Task<TransactionModel> OverwriteTransactionCategoriesForCurrentUser(int transactionId, int categoryId)
    {
        using var unitOfWork = _unitOfWork();

        var transaction = await FindTransaction(transactionId, unitOfWork);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        var category = await unitOfWork.CategoryRepository.FindSingle(item => item.Id == categoryId);
        if (category == null)
        {
            throw new NotFoundException(nameof(CategoryEntity));
        }

        transaction.Category = category;

        await unitOfWork.SaveAsync();

        return transaction.ToDto();
    }

    public async Task<TransactionModel> UpdateTransactionForCurrentUser(TransactionModel transactionModel)
    {
        using var unitOfWork = _unitOfWork();

        var existingTransaction = await FindTransaction(transactionModel.Id, unitOfWork);

        if (existingTransaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        existingTransaction.UpdateFromModel(transactionModel);

        unitOfWork.TransactionRepository.Update(existingTransaction);
        await unitOfWork.SaveAsync();

        existingTransaction.BankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(unitOfWork, existingTransaction.BankAccount);
        await unitOfWork.SaveAsync();

        return existingTransaction.ToDto();
    }

    private static async Task<TransactionEntity> FindTransaction(int transactionId, IUnitOfWork unitOfWork)
    {
        return await unitOfWork.TransactionRepository.FindSingleTracked(
            findQuery: item => item.Id == transactionId,
            includeProperties: new string[] {
                nameof(TransactionEntity.BankAccount),
                nameof(TransactionEntity.Category)
            });
    }
}
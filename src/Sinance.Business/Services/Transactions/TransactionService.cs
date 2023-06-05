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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserIdProvider _userIdProvider;

    public TransactionService(
        IUnitOfWork unitOfWork,
        IBankAccountCalculationService bankAccountCalculationService,
        IUserIdProvider userIdProvider)
    {
        _unitOfWork = unitOfWork;
        _bankAccountCalculationService = bankAccountCalculationService;
        _userIdProvider = userIdProvider;
    }

    public async Task<TransactionModel> ClearTransactionCategoriesForCurrentUser(int transactionId)
    {
        

        var transaction = await _unitOfWork.TransactionRepository.FindSingleTracked(
            item => item.Id == transactionId,
            includeProperties: nameof(TransactionEntity.TransactionCategories));

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        // Remove any previous assigned categories
        _unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);

        await _unitOfWork.SaveAsync();

        return transaction.ToDto();
    }

    public async Task<TransactionModel> CreateTransactionForCurrentUser(TransactionModel transactionModel)
    {
        

        var bankAccount = await _unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == transactionModel.BankAccountId);

        if (bankAccount == null)
        {
            throw new NotFoundException(nameof(BankAccountEntity));
        }

        var transactionEntity = transactionModel.ToNewEntity(_userIdProvider.GetCurrentUserId());
        _unitOfWork.TransactionRepository.Insert(transactionEntity);
        await _unitOfWork.SaveAsync();

        // First save the transaction, then recalculate the balance.
        bankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(_unitOfWork, bankAccount);
        await _unitOfWork.SaveAsync();

        return transactionEntity.ToDto();
    }

    public async Task DeleteTransactionForCurrentUser(int transactionId)
    {
        
        var transaction = await _unitOfWork.TransactionRepository.FindSingleTracked(item =>
            item.Id == transactionId,
            includeProperties: new string[] {
                nameof(TransactionEntity.BankAccount)
            });

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        if (transaction.TransactionCategories != null)
        {
            _unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);
        }

        _unitOfWork.TransactionRepository.Delete(transaction);
        await _unitOfWork.SaveAsync();

        transaction.BankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(_unitOfWork, transaction.BankAccount);

        await _unitOfWork.SaveAsync();
    }

    public async Task<List<TransactionModel>> GetBiggestExpensesForYearForCurrentUser(int year, int count, int skip, params int[] excludeCategoryIds)
    {
        

        excludeCategoryIds ??= new int[] { };

        var transactions = await _unitOfWork.TransactionRepository.FindTopAscending(findQuery: x =>
                    x.TransactionCategories.All(x => !excludeCategoryIds.Any(y => y == x.CategoryId)) &&
                    x.Date.Year == year,
                    orderByAscending: x => x.Amount,
                    count: count,
                    skip: skip,
                    includeProperties: new string[] {
                        nameof(TransactionEntity.TransactionCategories),
                        $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategoryEntity.Category)}"
                        });

        return transactions.ToDto().ToList();
    }

    public async Task<TransactionModel> GetTransactionByIdForCurrentUser(int transactionId)
    {
        

        var transaction = await FindTransaction(transactionId, _unitOfWork);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        return transaction.ToDto();
    }

    public async Task<List<TransactionModel>> GetTransactionsForBankAccountForCurrentUser(int bankAccountId, int count, int skip)
    {
        

        var transactions = await _unitOfWork.TransactionRepository
            .FindTopDescending(item => item.BankAccountId == bankAccountId,
                                orderByDescending: x => x.Date,
                                count: count,
                                skip: skip,
                                includeProperties: new string[] {
                                    nameof(TransactionEntity.TransactionCategories),
                                    $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategoryEntity.Category)}"
                                });

        return transactions.ToDto().ToList();
    }

    public async Task<List<TransactionModel>> GetTransactionsForMonthForCurrentUser(int year, int month)
    {
        

        var transactions = await _unitOfWork.TransactionRepository.FindAll(findQuery: x =>
                    x.Date.Year == year &&
                    x.Date.Month == month,
                    includeProperties: new string[] {
                        nameof(TransactionEntity.TransactionCategories),
                        $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategoryEntity.Category)}"
                        });

        return transactions.ToDto().ToList();
    }

    public async Task<TransactionModel> OverwriteTransactionCategoriesForCurrentUser(int transactionId, int categoryId)
    {
        

        var transaction = await FindTransaction(transactionId, _unitOfWork);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        var category = await _unitOfWork.CategoryRepository.FindSingle(item => item.Id == categoryId);

        if (category == null)
        {
            throw new NotFoundException(nameof(CategoryEntity));
        }

        // Remove any previous assigned categories
        _unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);

        // Insert the new link
        _unitOfWork.TransactionCategoryRepository.Insert(new TransactionCategoryEntity
        {
            TransactionId = transaction.Id,
            CategoryId = category.Id
        });

        await _unitOfWork.SaveAsync();

        transaction = await FindTransaction(transactionId, _unitOfWork);

        return transaction.ToDto();
    }

    public async Task<TransactionModel> UpdateTransactionForCurrentUser(TransactionModel transactionModel)
    {
        

        var existingTransaction = await FindTransaction(transactionModel.Id, _unitOfWork);

        if (existingTransaction == null)
        {
            throw new NotFoundException(nameof(TransactionEntity));
        }

        existingTransaction.UpdateFromModel(transactionModel);

        _unitOfWork.TransactionRepository.Update(existingTransaction);
        await _unitOfWork.SaveAsync();

        existingTransaction.BankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(_unitOfWork, existingTransaction.BankAccount);
        await _unitOfWork.SaveAsync();

        return existingTransaction.ToDto();
    }

    private static async Task<TransactionEntity> FindTransaction(int transactionId, IUnitOfWork unitOfWork)
    {
        return await unitOfWork.TransactionRepository.FindSingleTracked(
            findQuery: item => item.Id == transactionId,
            includeProperties: new string[] {
                nameof(TransactionEntity.BankAccount),
                nameof(TransactionEntity.TransactionCategories),
                $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategoryEntity.Category)}"
            });
    }
}
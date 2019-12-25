using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Transactions
{
    internal class TransactionService : ITransactionService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IBankAccountCalculationService _bankAccountCalculationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        internal TransactionService(
            Func<IUnitOfWork> unitOfWork,
            IBankAccountCalculationService bankAccountCalculationService,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _bankAccountCalculationService = bankAccountCalculationService;
            _authenticationService = authenticationService;
        }

        public async Task<TransactionModel> ClearTransactionCategoriesForCurrentUser(int transactionId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item => item.Id == transactionId &&
                           item.UserId == userId,
                           includeProperties: nameof(TransactionEntity.TransactionCategories));

            if (transaction == null)
            {
                throw new NotFoundException(nameof(TransactionEntity));
            }

            // Remove any previous assigned categories
            unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);

            await unitOfWork.SaveAsync();

            return transaction.ToDto();
        }

        public async Task<TransactionModel> CreateTransactionForCurrentUser(TransactionModel transactionModel)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.UserId == userId && x.Id == transactionModel.BankAccountId);

            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            var transactionEntity = transactionModel.ToNewEntity(userId);
            unitOfWork.TransactionRepository.Insert(transactionEntity);
            await unitOfWork.SaveAsync();

            // First save the transaction, then recalculate the balance.
            bankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(unitOfWork, bankAccount);
            await unitOfWork.SaveAsync();

            return transactionEntity.ToDto();
        }

        public async Task DeleteTransactionForCurrentUser(int transactionId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item =>
                item.Id == transactionId && item.UserId == userId,
                includeProperties: new string[] {
                    nameof(TransactionEntity.BankAccount)
                });

            if (transaction == null)
            {
                throw new NotFoundException(nameof(TransactionEntity));
            }

            if (transaction.TransactionCategories != null)
            {
                unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);
            }

            unitOfWork.TransactionRepository.Delete(transaction);
            await unitOfWork.SaveAsync();

            transaction.BankAccount.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(unitOfWork, transaction.BankAccount);

            await unitOfWork.SaveAsync();
        }

        public async Task<TransactionModel> GetTransactionByIdForCurrentUser(int transactionId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var transaction = await FindTransaction(transactionId, userId, unitOfWork);

            if (transaction == null)
            {
                throw new NotFoundException(nameof(TransactionEntity));
            }

            return transaction.ToDto();
        }

        public async Task<List<TransactionModel>> GetTransactionsForBankAccountForCurrentUser(int bankAccountId, int count, int skip)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var transactions = await unitOfWork.TransactionRepository
                .FindTopDescending(item => item.BankAccount.Id == bankAccountId && item.UserId == userId,
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
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var transactions = await unitOfWork.TransactionRepository.FindAll(findQuery: x =>
                        x.Date.Year == year &&
                        x.Date.Month == month &&
                        x.UserId == userId,
                        includeProperties: new string[] {
                            nameof(TransactionEntity.TransactionCategories),
                            $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategoryEntity.Category)}"
                            });

            return transactions.ToDto().ToList();
        }

        public async Task<TransactionModel> OverwriteTransactionCategoriesForCurrentUser(int transactionId, int categoryId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var transaction = await FindTransaction(transactionId, userId, unitOfWork);

            if (transaction == null)
            {
                throw new NotFoundException(nameof(TransactionEntity));
            }

            var category = await unitOfWork.CategoryRepository.FindSingle(item => item.Id == categoryId &&
                                                                        item.UserId == userId);

            if (category == null)
            {
                throw new NotFoundException(nameof(CategoryEntity));
            }

            // Remove any previous assigned categories
            unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);

            // Insert the new link
            unitOfWork.TransactionCategoryRepository.Insert(new TransactionCategoryEntity
            {
                TransactionId = transaction.Id,
                CategoryId = category.Id
            });

            await unitOfWork.SaveAsync();

            transaction = await FindTransaction(transactionId, userId, unitOfWork);

            return transaction.ToDto();
        }

        public async Task<TransactionModel> UpdateTransactionForCurrentUser(TransactionModel transactionModel)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var existingTransaction = await FindTransaction(transactionModel.Id, userId, unitOfWork);

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

        private static async Task<TransactionEntity> FindTransaction(int transactionId, int userId, IUnitOfWork unitOfWork)
        {
            return await unitOfWork.TransactionRepository.FindSingleTracked(
                findQuery: item => item.Id == transactionId && item.UserId == userId,
                includeProperties: new string[] {
                    nameof(TransactionEntity.BankAccount),
                    nameof(TransactionEntity.TransactionCategories),
                    $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategoryEntity.Category)}"
                });
        }
    }
}
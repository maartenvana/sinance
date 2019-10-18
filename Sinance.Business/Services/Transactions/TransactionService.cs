using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Handlers;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly Func<IUnitOfWork> _unitOfWork;

        public TransactionService(Func<IUnitOfWork> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TransactionModel> ClearTransactionCategories(int userId, int transactionId)
        {
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

        public async Task<TransactionModel> CreateTransaction(int userId, TransactionModel transactionModel)
        {
            using var unitOfWork = _unitOfWork();

            var bankAccount = unitOfWork.BankAccountRepository.FindSingle(x => x.UserId == userId && x.Id == transactionModel.BankAccountId);

            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            var transactionEntity = transactionModel.ToNewEntity(userId);
            unitOfWork.TransactionRepository.Insert(transactionEntity);

            await unitOfWork.SaveAsync();

            await TransactionHandler.UpdateCurrentBalance(unitOfWork, bankAccount.Id, userId);

            return transactionEntity.ToDto();
        }

        public async Task DeleteTransactionForUser(int userId, int transactionId)
        {
            using var unitOfWork = _unitOfWork();
            var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item => item.Id == transactionId && item.UserId == userId);

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

            await TransactionHandler.UpdateCurrentBalance(unitOfWork, transaction.BankAccountId, userId);
        }

        public async Task<TransactionModel> GetTransactionByIdForUserId(int userId, int transactionId)
        {
            using var unitOfWork = _unitOfWork();

            var transaction = await unitOfWork.TransactionRepository.FindSingle(item => item.Id == transactionId && item.UserId == userId);

            if (transaction == null)
            {
                throw new NotFoundException(nameof(TransactionEntity));
            }

            return transaction.ToDto();
        }

        public async Task<IEnumerable<TransactionModel>> GetTransactionsForBankAccount(int currentUserId, int bankAccountId, int count, int skip)
        {
            using var unitOfWork = _unitOfWork();

            var transactions = await unitOfWork.TransactionRepository
                .FindTopDescending(item => item.BankAccount.Id == bankAccountId && item.UserId == currentUserId,
                                    orderByDescending: x => x.Date,
                                    count: count,
                                    skip: skip,
                                    includeProperties: new string[] {
                                        nameof(TransactionEntity.TransactionCategories),
                                        $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategory.Category)}"
                                    });

            return transactions.ToDto();
        }

        public async Task<IEnumerable<TransactionModel>> GetTransactionsForUserForMonth(int userId, int year, int month)
        {
            using var unitOfWork = _unitOfWork();

            var transactions = await unitOfWork.TransactionRepository.FindAll(findQuery: x =>
                        x.Date.Year == year &&
                        x.Date.Month == month &&
                        x.UserId == userId,
                        includeProperties: new string[] {
                            nameof(TransactionEntity.TransactionCategories),
                            $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(CategoryEntity.ParentCategory)}"
                            });

            return transactions.ToDto();
        }

        public async Task<TransactionModel> OverwriteTransactionCategories(int userId, int transactionId, int categoryId)
        {
            using var unitOfWork = _unitOfWork();

            var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item => item.Id == transactionId &&
                           item.UserId == userId,
                           includeProperties: nameof(TransactionEntity.TransactionCategories));

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
            unitOfWork.TransactionCategoryRepository.Insert(new TransactionCategory
            {
                TransactionId = transaction.Id,
                CategoryId = category.Id
            });

            await unitOfWork.SaveAsync();

            return transaction.ToDto();
        }

        public async Task<TransactionModel> UpdateTransaction(int userId, TransactionModel transactionModel)
        {
            using var unitOfWork = _unitOfWork();

            var existingTransaction = await unitOfWork.TransactionRepository.FindSingleTracked(item =>
                item.Id == transactionModel.Id && item.UserId == userId);

            if (existingTransaction == null)
            {
                throw new NotFoundException(nameof(TransactionEntity));
            }

            existingTransaction.UpdateFromModel(transactionModel);

            unitOfWork.TransactionRepository.Update(existingTransaction);
            await unitOfWork.SaveAsync();

            await TransactionHandler.UpdateCurrentBalance(unitOfWork, existingTransaction.BankAccountId, userId);

            return existingTransaction.ToDto();
        }
    }
}
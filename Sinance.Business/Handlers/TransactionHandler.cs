using Sinance.Common;
using Sinance.Domain.Entities;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers
{
    /// <summary>
    /// Transaction Handler for transaction related operation
    /// </summary>
    public static class TransactionHandler
    {
        private const string _transactionCacheKeyFormat = "AllTransactions_User_{0}";

        /// <summary>
        /// Clears the cache for this user's transactions
        /// </summary>
        /// <param name="userId">User to clear the transaction cache for</param>
        public static void ClearTransactionsForUserCached(string userId)
        {
            var cacheKey = string.Format(CultureInfo.CurrentCulture, _transactionCacheKeyFormat, userId);
            FinanceCacheHandler.ClearCache(cacheKey);
        }

        /// <summary>
        /// Retrieves the user's transactions cached
        /// </summary>
        /// <param name="genericRepository">Generic repository to use for queries</param>
        /// <param name="userId">User to get the transactions for</param>
        /// <returns>List of cached transactions</returns>
        public static IList<Transaction> TransactionsForUserCached(IUnitOfWork unitOfWork, int userId)
        {
            var cacheKey = string.Format(CultureInfo.CurrentCulture, _transactionCacheKeyFormat, userId);

            return FinanceCacheHandler.Cache(key: cacheKey,
                contentAction: () =>
                {
                    return unitOfWork.TransactionRepository.FindAllTracked(item => item.UserId == userId);
                },
                slidingExpiration: true,
                expirationTimeSpan: new TimeSpan(0, 1, 0, 0));
        }

        /// <summary>
        /// Updates the current balance for the given bank account
        /// </summary>
        /// <param name="genericRepository">Generic repository to use</param>
        /// <param name="bankAccountId">Bank account to use</param>
        /// <param name="userId">Id of the user</param>
        public static async Task UpdateCurrentBalance(IUnitOfWork unitOfWork, int bankAccountId, int userId)
        {
            var bankAccount = unitOfWork.BankAccountRepository.FindSingleTracked(item => item.Id == bankAccountId &&
                                                                                        item.UserId == userId);
            if (bankAccount != null)
            {
                var currentBalance = bankAccount.StartBalance;

                var transactions = unitOfWork.TransactionRepository.FindAllTracked(item => item.BankAccountId == bankAccount.Id).ToList();

                if (transactions.Any())
                    currentBalance += transactions.Sum(item => item.Amount);

                bankAccount.CurrentBalance = currentBalance;
                unitOfWork.BankAccountRepository.Update(bankAccount);
                await unitOfWork.SaveAsync();
            }
        }
    }
}
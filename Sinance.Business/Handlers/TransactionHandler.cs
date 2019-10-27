﻿using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// Updates the current balance for the given bank account
        /// </summary>
        /// <param name="genericRepository">Generic repository to use</param>
        /// <param name="bankAccountId">Bank account to use</param>
        /// <param name="userId">Id of the user</param>
        public static async Task<decimal> CalculateCurrentBalanceForBankAccount(IUnitOfWork unitOfWork, BankAccountEntity bankAccount)
        {
            var currentBalance = bankAccount.StartBalance;

            var transactionSum = await unitOfWork.TransactionRepository.Sum(
                findQuery: x => x.BankAccountId == bankAccount.Id,
                sumQuery: x => x.Amount);

            return currentBalance + transactionSum;
        }

        /// <summary>
        /// Clears the cache for this user's transactions
        /// </summary>
        /// <param name="userId">User to clear the transaction cache for</param>
        public static void ClearTransactionsForUserCached(string userId)
        {
            var cacheKey = string.Format(CultureInfo.CurrentCulture, _transactionCacheKeyFormat, userId);
            SinanceCacheHandler.ClearCache(cacheKey);
        }

        /// <summary>
        /// Retrieves the user's transactions cached
        /// </summary>
        /// <param name="genericRepository">Generic repository to use for queries</param>
        /// <param name="userId">User to get the transactions for</param>
        /// <returns>List of cached transactions</returns>
        public static async Task<IList<TransactionEntity>> TransactionsForUserCached(IUnitOfWork unitOfWork, int userId)
        {
            var cacheKey = string.Format(CultureInfo.CurrentCulture, _transactionCacheKeyFormat, userId);

            return await SinanceCacheHandler.Cache(key: cacheKey,
                contentAction: async () =>
                {
                    return await unitOfWork.TransactionRepository.FindAll(item => item.UserId == userId);
                },
                slidingExpiration: true,
                expirationTimeSpan: new TimeSpan(0, 1, 0, 0));
        }
    }
}
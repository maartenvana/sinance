using Sinance.Domain.Entities;
using Sinance.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers
{
    /// <summary>
    /// Category mapping for mapping categories to transactions
    /// </summary>
    public static class CategoryHandler
    {
        /// <summary>
        /// Maps categories to transactions
        /// </summary>
        /// <param name="genericRepository">Generic repository to use</param>
        /// <param name="categoryId">Category Id to map</param>
        /// <param name="transactions">Transactions to map</param>
        public static async Task MapCategoryToTransactions(IUnitOfWork unitOfWork, int categoryId, IEnumerable<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                foreach (var transactionCategory in transaction.TransactionCategories.Where(transactionCategory => transactionCategory.CategoryId != categoryId))
                {
                    unitOfWork.TransactionCategoryRepository.Delete(transactionCategory);
                }

                if (transaction.TransactionCategories.All(item => item.CategoryId != categoryId))
                {
                    unitOfWork.TransactionCategoryRepository.Insert(new TransactionCategory
                    {
                        TransactionId = transaction.Id,
                        Amount = null,
                        CategoryId = categoryId
                    });
                }
            }

            await unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Maps categories to transactions
        /// </summary>
        /// <param name="categoryMappings">CategoryMappings to use</param>
        /// <param name="transactions">Transactions to map to</param>
        /// <returns>List of transactions that can map</returns>

        /// <summary>
        /// Unmaps the category from the given transactions
        /// </summary>
        /// <param name="genericRepository">Generic repository to use</param>
        /// <param name="categoryId">Category Id to unmap</param>
        /// <param name="transactions">Transactions to unmap</param>
        public static async Task RemoveTransactionMappingFromCategory(IUnitOfWork unitOfWork, int categoryId, IEnumerable<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories.Where(item => item.CategoryId == categoryId));
            }

            await unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Sets the transactioncategory to the first found match of the given category mappings
        /// </summary>
        /// <param name="transactions">Transaction to set category for</param>
        /// <param name="categoryMappings">Mappings to use</param>
        public static void SetTransactionCategories(IEnumerable<Transaction> transactions, IList<CategoryMapping> categoryMappings)
        {
            foreach (var transaction in transactions)
            {
                foreach (var mapping in categoryMappings)
                {
                    var isMatch = false;

                    switch (mapping.ColumnTypeId)
                    {
                        case ColumnType.Description:
                            isMatch = MatchString(transaction.Description, mapping.MatchValue);
                            break;

                        case ColumnType.Name:
                            isMatch = MatchString(transaction.Name, mapping.MatchValue);
                            break;

                        case ColumnType.DestinationAccount:
                            isMatch = MatchString(transaction.DestinationAccount, mapping.MatchValue);
                            break;

                        default:
                            break;
                    }

                    if (isMatch)
                    {
                        transaction.TransactionCategories = new List<TransactionCategory>
                        {
                            new TransactionCategory
                            {
                                CategoryId = mapping.CategoryId,
                                TransactionId = transaction.Id,
                                Category = mapping.Category
                            }
                        };
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Matches a transaction value with a matchvalue from a category mapping
        /// </summary>
        /// <param name="transactionValue">Transaction value to check</param>
        /// <param name="matchValue">Value to check against</param>
        /// <returns>If its a match or not</returns>
        private static bool MatchString(string transactionValue, string matchValue)
        {
            if (!string.IsNullOrEmpty(transactionValue))
                return transactionValue.ToUpperInvariant().Contains(matchValue.ToUpperInvariant());

            return false;
        }
    }
}
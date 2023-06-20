using Sinance.Business.Extensions;
using Sinance.Communication.Model.Import;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers;

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
    public static async Task MapCategoryToTransactions(SinanceContext context, int categoryId, IEnumerable<TransactionEntity> transactions)
    {
        foreach (var transaction in transactions)
        {
            foreach (var transactionCategory in transaction.TransactionCategories.Where(transactionCategory => transactionCategory.CategoryId != categoryId))
            {
                context.TransactionCategories.Remove(transactionCategory);
            }

            if (transaction.TransactionCategories.All(item => item.CategoryId != categoryId))
            {
                await context.TransactionCategories.AddAsync(new TransactionCategoryEntity
                {
                    TransactionId = transaction.Id,
                    Amount = null,
                    CategoryId = categoryId
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Unmaps the category from the given transactions
    /// </summary>
    /// <param name="genericRepository">Generic repository to use</param>
    /// <param name="categoryId">Category Id to unmap</param>
    /// <param name="transactions">Transactions to unmap</param>
    public static async Task RemoveTransactionMappingFromCategory(SinanceContext context, int categoryId, IEnumerable<TransactionEntity> transactions)
    {
        foreach (var transaction in transactions)
        {
            context.TransactionCategories.RemoveRange(transaction.TransactionCategories.Where(item => item.CategoryId == categoryId));
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Sets the transactioncategory to the first found match of the given category mappings
    /// </summary>
    /// <param name="transactions">Transaction to set category for</param>
    /// <param name="categoryMappings">Mappings to use</param>
    public static void SetTransactionCategories(IEnumerable<TransactionModel> transactions, IList<CategoryMappingEntity> categoryMappings)
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
                    transaction.Categories = new List<TransactionCategoryModel>
                    {
                        new TransactionCategoryEntity
                        {
                            CategoryId = mapping.CategoryId,
                            TransactionId = transaction.Id,
                            Category = mapping.Category
                        }.ToDto()
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
        return !string.IsNullOrEmpty(transactionValue) && transactionValue.ToUpperInvariant().Contains(matchValue.ToUpperInvariant());
    }
}
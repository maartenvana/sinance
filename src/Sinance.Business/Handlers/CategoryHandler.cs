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
                    transaction.Category = new TransactionCategoryModel
                    {
                        CategoryId = mapping.CategoryId,
                        Name = mapping.Category.Name,
                        ColorCode = mapping.Category.ColorCode
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
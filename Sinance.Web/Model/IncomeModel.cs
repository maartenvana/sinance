using Sinance.Domain.Entities;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Incomes model
    /// </summary>
    public class IncomeModel
    {
        /// <summary>
        /// Volatile expenses report for two months
        /// </summary>
        public BimonthlyIncomeReport BimonthlyIncomeReport { get; set; }

        /// <summary>
        /// Uncategorized transactions
        /// </summary>
        public IEnumerable<Transaction> UncategorizedTransactionsThisMonth { get; set; }
    }
}
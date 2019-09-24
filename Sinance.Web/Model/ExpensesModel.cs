using Sinance.Domain.Entities;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model for the overall overview
    /// </summary>
    public class ExpensesModel
    {
        /// <summary>
        /// Regular expenses report for two months
        /// </summary>
        public BimonthlyExpenseReport RegularBimonthlyExpenseReport { get; set; }

        /// <summary>
        /// Uncategorized transactions
        /// </summary>
        public IEnumerable<Transaction> UncategorizedTransactionsThisMonth { get; set; }

        /// <summary>
        /// Volatile expenses report for two months
        /// </summary>
        public BimonthlyExpenseReport VolatileBimonthlyExpenseReport { get; set; }
    }
}
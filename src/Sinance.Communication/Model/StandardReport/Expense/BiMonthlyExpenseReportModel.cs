using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.Communication.Model.StandardReport.Expense
{
    public class BiMonthlyExpenseReportModel
    {
        /// <summary>
        /// Regular expenses report for two months
        /// </summary>
        public BimonthlyExpenseReportItem RegularBimonthlyExpenseReport { get; set; }

        /// <summary>
        /// Uncategorized transactions
        /// </summary>
        public IEnumerable<TransactionModel> UncategorizedTransactions { get; set; }

        /// <summary>
        /// Volatile expenses report for two months
        /// </summary>
        public BimonthlyExpenseReportItem VolatileBimonthlyExpenseReport { get; set; }
    }
}
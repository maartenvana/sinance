using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.Communication.Model.StandardReport.Income
{
    /// <summary>
    /// Incomes model
    /// </summary>
    public class BiMonthlyIncomeReportModel
    {
        /// <summary>
        /// Volatile expenses report for two months
        /// </summary>
        public BimonthlyIncomeReportItem BimonthlyIncomeReport { get; set; }

        /// <summary>
        /// Uncategorized transactions
        /// </summary>
        public IEnumerable<TransactionModel> UncategorizedTransactions { get; set; }
    }
}
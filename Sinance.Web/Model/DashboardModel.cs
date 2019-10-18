using Sinance.Communication.BankAccount;
using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model for the overall overview
    /// </summary>
    public class DashboardModel
    {
        /// <summary>
        /// List of bank accounts to show
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<BankAccountModel> BankAccounts { get; set; }

        public IList<TransactionModel> BiggestExpenses { get; set; }

        public decimal LastMonthExpenses { get; set; }

        public decimal LastMonthIncome { get; set; }

        public decimal LastMonthProfitLoss { get; set; }
    }
}
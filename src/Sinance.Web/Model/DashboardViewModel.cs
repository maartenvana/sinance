using Sinance.Communication.Model.BankAccount;
using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model for the overall overview
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// List of bank accounts to show
        /// </summary>
        public IList<BankAccountModel> BankAccounts { get; set; }

        public IList<TransactionModel> BiggestExpenses { get; set; }

        public decimal LastMonthExpenses { get; set; }

        public decimal LastMonthIncome { get; set; }

        public decimal LastMonthProfitLoss { get; set; }
    }
}
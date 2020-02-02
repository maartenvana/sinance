using Sinance.Communication.Model.BankAccount;
using System.Collections.Generic;

namespace Sinance.Communication.Model.StandardReport.Yearly
{
    public class YearlyOverviewModel
    {
        public int Year { get; set; }
        public YearBalance TotalBalance { get; set; }

        public Dictionary<BankAccountModel, YearBalance> BalancePerBankAccount { get; set; } = new Dictionary<BankAccountModel, YearBalance>();
    }
}
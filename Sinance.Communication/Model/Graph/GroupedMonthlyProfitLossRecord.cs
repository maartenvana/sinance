using Sinance.Communication.Model.BankAccount;
using System.Collections.Generic;

namespace Sinance.Communication.Model.Graph
{
    public class GroupedMonthlyProfitLossRecord
    {
        public ICollection<decimal> ProfitPerMonth { get; set; }

        public BankAccountType AccountType { get; set; }
    }
}

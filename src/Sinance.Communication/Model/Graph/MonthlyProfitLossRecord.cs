using Sinance.Communication.Model.BankAccount;
using System.Collections.Generic;

namespace Sinance.Communication.Model.Graph
{
    public class MonthlyProfitLossRecord
    {
        public ICollection<decimal[]> ProfitPerMonth { get; set; }

        public BankAccountType? AccountTypeGroup { get; set; }
    }
}

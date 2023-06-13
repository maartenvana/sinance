using Sinance.Communication.Model.BankAccount;
using System.Collections.Generic;

namespace Sinance.Communication.Model.Graph;

public class BalanceHistoryRecord
{
    public List<decimal[]> BalanceHistory { get; set; }

    public BankAccountType? AccountTypeGroup { get; set; }
}
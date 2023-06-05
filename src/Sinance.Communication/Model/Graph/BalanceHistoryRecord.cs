using Sinance.Communication.Model.BankAccount;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Communication.Model.Graph;

public class BalanceHistoryRecord
{
    public List<decimal[]> BalanceHistory { get; set; }
    
    public BankAccountType? AccountTypeGroup { get; set; }
}

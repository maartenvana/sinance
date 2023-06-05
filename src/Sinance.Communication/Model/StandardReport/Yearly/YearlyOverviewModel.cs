using Sinance.Communication.Model.BankAccount;
using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.Communication.Model.StandardReport.Yearly;

public class YearlyOverviewModel
{
    public Dictionary<BankAccountModel, YearBalance> BalancePerBankAccount { get; set; } = new Dictionary<BankAccountModel, YearBalance>();
    public Dictionary<BankAccountType, YearAmountAndPercentage> BalancePerBankAccountType { get; set; } = new Dictionary<BankAccountType, YearAmountAndPercentage>();
    public IList<TransactionModel> BiggestExpenses { get; set; }
    public YearBalance TotalBalance { get; set; }
    public int Year { get; set; }
}
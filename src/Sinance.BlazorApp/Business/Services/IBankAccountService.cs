using Sinance.BlazorApp.Business.Model.BankAccount;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Services;

public interface IBankAccountService
{
    BankAccountModel GetBankAccount(int id);
    List<BankAccountModel> GetAllActiveBankAccounts();
    List<BankAccountModel> GetAllBankAccounts();
    Task RecalculateBalanceAsync(int bankAccountId);
}
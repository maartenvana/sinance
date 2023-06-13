using Sinance.BlazorApp.Business.Model.BankAccount;
using System.Collections.Generic;

namespace Sinance.BlazorApp.Business.Services;

public interface IBankAccountService
{
    List<BankAccountModel> GetAllActiveBankAccounts();

    List<BankAccountModel> GetAllBankAccounts();
}
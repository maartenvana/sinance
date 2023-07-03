using Sinance.Communication.Model.BankAccount;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts
{
    public interface IBankAccountService
    {
        Task<BankAccountModel> CreateBankAccountForCurrentUser(BankAccountModel model);

        Task DeleteBankAccountByIdForCurrentUser(int accountId);

        Task<List<BankAccountModel>> GetActiveBankAccountsForCurrentUser();

        Task<List<BankAccountModel>> GetAllBankAccountsForCurrentUser();

        Task<BankAccountModel> GetBankAccountByIdForCurrentUser(int bankAccountId);

        Task<BankAccountModel> UpdateBankAccountForCurrentUser(BankAccountModel model);
    }
}
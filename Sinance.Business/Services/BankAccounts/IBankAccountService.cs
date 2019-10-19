using Sinance.Communication.BankAccount;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts
{
    public interface IBankAccountService
    {
        Task<BankAccountModel> CreateBankAccountForCurrentUser(BankAccountModel model);

        Task DeleteBankAccountByIdForCurrentUser(int accountId);

        Task<IList<BankAccountModel>> GetActiveBankAccountsForCurrentUser();

        Task<IList<BankAccountModel>> GetAllBankAccountsForCurrentUser();

        Task<BankAccountModel> GetBankAccountByIdForCurrentUser(int bankAccountId);

        Task<BankAccountModel> UpdateBankAccountForCurrentUser(BankAccountModel model);
    }
}
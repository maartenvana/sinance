using Sinance.Communication.BankAccount;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts
{
    public interface IBankAccountService
    {
        Task<BankAccountModel> CreateBankAccount(int userId, BankAccountModel model);

        Task DeleteBankAccountByIdForUser(int userId, int accountId);

        Task<IList<BankAccountModel>> GetActiveBankAccountsForUser(int userId);

        Task<IList<BankAccountModel>> GetAllBankAccountsForUser(int userId);

        Task<BankAccountModel> GetBankAccountByIdForUser(int userId, int bankAccountId);

        Task<BankAccountModel> UpdateBankAccount(int currentUserId, BankAccountModel model);
    }
}
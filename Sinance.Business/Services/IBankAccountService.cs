using Sinance.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services
{
    public interface IBankAccountService
    {
        Task<IList<BankAccount>> GetActiveBankAccountsForCurrentUser();

        Task<IList<BankAccount>> GetAllBankAccountsForCurrentUser();
    }
}
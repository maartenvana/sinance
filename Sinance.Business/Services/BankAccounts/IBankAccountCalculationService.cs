using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts
{
    public interface IBankAccountCalculationService
    {
        Task<decimal> CalculateCurrentBalanceForBankAccount(IUnitOfWork unitOfWork, BankAccountEntity bankAccount);

        Task UpdateCurrentBalanceForBankAccount(int bankAccountId, int userId);
    }
}
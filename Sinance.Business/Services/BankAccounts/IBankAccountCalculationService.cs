using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts
{
    internal interface IBankAccountCalculationService
    {
        Task<decimal> CalculateCurrentBalanceForBankAccount(IUnitOfWork unitOfWork, BankAccountEntity bankAccount);

        Task UpdateCurrentBalanceForBankAccount(int bankAccountId, int userId);
    }
}
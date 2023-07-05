using Sinance.Business.Exceptions;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts;

public class BankAccountCalculationService : IBankAccountCalculationService
{
    public async Task<decimal> CalculateCurrentBalanceForBankAccount(IUnitOfWork unitOfWork, BankAccountEntity bankAccount)
    {
        var transactionSum = await unitOfWork.TransactionRepository.Sum(
            findQuery: x => x.BankAccountId == bankAccount.Id,
            sumQuery: x => x.Amount);

        return bankAccount.StartBalance + transactionSum;
    }

    public async Task UpdateCurrentBalanceForBankAccount(IUnitOfWork unitOfWork, int bankAccountId)
    {
        var bankAccount = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == bankAccountId);

        if (bankAccount == null)
        {
            throw new NotFoundException(nameof(BankAccountEntity));
        }

        bankAccount.CurrentBalance = await CalculateCurrentBalanceForBankAccount(unitOfWork, bankAccount);
    }
}
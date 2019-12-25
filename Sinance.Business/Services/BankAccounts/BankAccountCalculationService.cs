using Sinance.Business.Exceptions;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts
{
    internal class BankAccountCalculationService : IBankAccountCalculationService
    {
        private readonly Func<IUnitOfWork> _unitOfWork;

        public BankAccountCalculationService(Func<IUnitOfWork> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> CalculateCurrentBalanceForBankAccount(IUnitOfWork unitOfWork, BankAccountEntity bankAccount)
        {
            var transactionSum = await unitOfWork.TransactionRepository.Sum(
                findQuery: x => x.BankAccountId == bankAccount.Id,
                sumQuery: x => x.Amount);

            return bankAccount.StartBalance + transactionSum;
        }

        public async Task UpdateCurrentBalanceForBankAccount(int bankAccountId, int userId)
        {
            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.UserId == userId && x.Id == bankAccountId);

            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            bankAccount.CurrentBalance = await CalculateCurrentBalanceForBankAccount(unitOfWork, bankAccount);

            await unitOfWork.SaveAsync();
        }
    }
}
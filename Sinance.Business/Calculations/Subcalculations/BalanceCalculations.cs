using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations.Subcalculations
{
    public static class BalanceCalculations
    {
        public static async Task<decimal> TotalBalanceForBankAccountBeforeDate(IUnitOfWork unitOfWork, DateTime date, BankAccountModel bankAccount)
        {
            var totalTransactionBalance = await unitOfWork.TransactionRepository
                .Sum(x => x.BankAccountId == bankAccount.Id && x.Date < date, x => x.Amount);

            return bankAccount.StartBalance + totalTransactionBalance;
        }

        public static async Task<decimal> TotalBalanceBeforeDate(IUnitOfWork unitOfWork, DateTime date)
        {
            var totalStartBalance = await unitOfWork.BankAccountRepository.Sum(x => !x.Disabled, x => x.StartBalance);
            var totalTransactionBalance = await unitOfWork.TransactionRepository.Sum(x => x.Date < date && !x.BankAccount.Disabled, x => x.Amount);

            return totalStartBalance + totalTransactionBalance;
        }
    }
}
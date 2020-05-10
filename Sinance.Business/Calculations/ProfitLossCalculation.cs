using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.BankAccount;
using Sinance.Communication.Model.Graph;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class ProfitLossCalculation : IProfitLossCalculation
    {
        private readonly Func<IUnitOfWork> _unitOfWork;
        private readonly IBankAccountService _bankAccountService;

        public ProfitLossCalculation(IBankAccountService bankAccountService, Func<IUnitOfWork> unitOfWork)
        {
            _bankAccountService = bankAccountService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<GroupedMonthlyProfitLossRecord>> CalculateProfitLosstPerMonthForYearGrouped(int year)
        {
            var records = new List<GroupedMonthlyProfitLossRecord>();

            using var unitOfWork = _unitOfWork();

            // No need to sort this list, we loop through it by month numbers
            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

            var groupedBankAccounts = bankAccounts.GroupBy(x => x.AccountType);

            foreach (var bankAccountGroup in groupedBankAccounts)
            {
                var bankAccountIds = bankAccountGroup.Select(x => x.Id);

                var transactionsPerMonth = (await unitOfWork.TransactionRepository
                    .FindAll(x => x.Date.Year == year && bankAccountIds.Any(y => y == x.BankAccountId)))
                    .GroupBy(x => x.Date.Month)
                    .ToList();

                var profitPerMonth = new List<decimal>();

                for (var month = 1; month <= 12; month++)
                {
                    var transactions = transactionsPerMonth.SingleOrDefault(item => item.Key == month);
                    profitPerMonth.Add(transactions?.Sum(item => item.Amount) ?? 0);
                }

                records.Add(new GroupedMonthlyProfitLossRecord
                {
                    AccountType = bankAccountGroup.Key,
                    ProfitPerMonth = profitPerMonth
                });
            }

            return records;
        }
        public async Task<IEnumerable<decimal>> CalculateProfitLosstPerMonthForYear(int year)
        {
            using var unitOfWork = _unitOfWork();

            // No need to sort this list, we loop through it by month numbers
            var transactionsPerMonth = (await unitOfWork.TransactionRepository
                .FindAll(item => item.Date.Year == year))
                .GroupBy(item => item.Date.Month)
                .ToList();

            var profitPerMonth = new List<decimal>();

            for (var month = 1; month <= 12; month++)
            {
                var transactions = transactionsPerMonth.SingleOrDefault(item => item.Key == month);
                profitPerMonth.Add(transactions?.Sum(item => item.Amount) ?? 0);
            }

            return profitPerMonth;
        }
    }
}
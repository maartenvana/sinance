using Sinance.Business.Extensions;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.Graph;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class ProfitLossCalculation : IProfitCalculation
    {
        private readonly Func<IUnitOfWork> _unitOfWork;
        private readonly IBankAccountService _bankAccountService;

        public ProfitLossCalculation(IBankAccountService bankAccountService, Func<IUnitOfWork> unitOfWork)
        {
            _bankAccountService = bankAccountService;
            _unitOfWork = unitOfWork;
        }

        private static List<decimal[]> GetProfitPerMonth(DateTime startDate, DateTime endDate, List<IGrouping<DateTime, TransactionEntity>> transactionsPerMonth)
        {
            var profitPerMonth = new List<decimal[]>();

            for (var month = 0; month <= startDate.GetMonthsBetween(endDate); month++)
            {
                var currentDate = startDate.AddMonths(month).BeginningOfMonth();

                var transactions = transactionsPerMonth.SingleOrDefault(item => item.Key == currentDate);
                profitPerMonth.Add(new decimal[] {
                        Convert.ToDecimal(((currentDate - DateTimeOffset.UnixEpoch).TotalMilliseconds)),
                        transactions?.Sum(item => item.Amount) ?? 0
                    });
            }

            return profitPerMonth;
        }

        public async Task<List<MonthlyProfitLossRecord>> CalculateMonthlyProfit(DateTime startDate, DateTime endDate)
        {
            using var unitOfWork = _unitOfWork();

            // No need to sort this list, we loop through it by month numbers
            var transactionsPerMonth = (await unitOfWork.TransactionRepository
                .FindAll(item => item.Date >= startDate && item.Date <= endDate))
                .GroupBy(item => new DateTime(item.Date.Year, item.Date.Month, 1))
                .ToList();

            var profitPerMonth = GetProfitPerMonth(startDate, endDate, transactionsPerMonth);

            return new List<MonthlyProfitLossRecord>
            {
                new MonthlyProfitLossRecord
                {
                    AccountTypeGroup = null,
                    ProfitPerMonth = profitPerMonth
                }
            };
        }

        public async Task<List<MonthlyProfitLossRecord>> CalculateMonthlyProfitGrouped(DateTime startDate, DateTime endDate)
        {
            var records = new List<MonthlyProfitLossRecord>();

            using var unitOfWork = _unitOfWork();

            // No need to sort this list, we loop through it by month numbers
            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

            var groupedBankAccounts = bankAccounts.GroupBy(x => x.AccountType);

            foreach (var bankAccountGroup in groupedBankAccounts)
            {
                var bankAccountIds = bankAccountGroup.Select(x => x.Id);

                var transactionsPerMonth = (await unitOfWork.TransactionRepository
                    .FindAll(x => x.Date >= startDate && x.Date <= endDate && bankAccountIds.Any(y => y == x.BankAccountId)))
                    .GroupBy(x => x.Date.BeginningOfMonth())
                    .ToList();

                var profitPerMonth = GetProfitPerMonth(startDate, endDate, transactionsPerMonth);

                records.Add(new MonthlyProfitLossRecord
                {
                    AccountTypeGroup = bankAccountGroup.Key,
                    ProfitPerMonth = profitPerMonth
                });
            }

            return records;
        }
    }
}
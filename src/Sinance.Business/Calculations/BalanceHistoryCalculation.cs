using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.Graph;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class BalanceHistoryCalculation : IBalanceHistoryCalculation
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public BalanceHistoryCalculation(
            Func<IUnitOfWork> unitOfWork,
            IBankAccountService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _bankAccountService = bankAccountService;
        }

        public async Task<List<BalanceHistoryRecord>> BalanceHistoryForYear(int year, IEnumerable<int> includeBankAccounts)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59, 999);

            return new List<BalanceHistoryRecord>
            {
                new BalanceHistoryRecord
                {
                    BalanceHistory = await CalculateBalanceHistory(startDate, endDate, includeBankAccounts)
                }
            };
        }

        public async Task<List<BalanceHistoryRecord>> BalanceHistoryForYearGroupedByType(int year, IEnumerable<int> includeBankAccounts)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59, 999);

            return await GetBalanceHistoryRecordsGroupedByType(includeBankAccounts, startDate, endDate);
        }

        public async Task<List<BalanceHistoryRecord>> BalanceHistoryFromMonthsInPastGroupedByType(int monthsInPast, IEnumerable<int> includeBankAccounts)
        {
            var startDate = DateTime.Now.AddMonths(monthsInPast * -1);
            var endDate = DateTime.Now;

            return await GetBalanceHistoryRecordsGroupedByType(includeBankAccounts, startDate, endDate);
        }

        private async Task<List<BalanceHistoryRecord>> GetBalanceHistoryRecordsGroupedByType(IEnumerable<int> includeBankAccounts, DateTime startDate, DateTime endDate)
        {
            var userBankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

            // This might seem backwards, but this way we validate if the bankaccounts given are our own
            var bankAccounts = includeBankAccounts.Any() ? userBankAccounts.Where(item => includeBankAccounts.Any(y => y == item.Id)).ToList() : userBankAccounts;

            var groupedBankAccounts = bankAccounts.GroupBy(x => x.AccountType);

            var groupedBalanceHistoryRecords = new List<BalanceHistoryRecord>();
            foreach (var bankAccountGroup in groupedBankAccounts)
            {
                var balanceHistory = await CalculateBalanceHistory(startDate, endDate, bankAccountGroup.Select(x => x.Id));

                groupedBalanceHistoryRecords.Add(new BalanceHistoryRecord
                {
                    AccountTypeGroup = bankAccountGroup.Key,
                    BalanceHistory = balanceHistory,
                });
            }

            return groupedBalanceHistoryRecords;
        }

        public async Task<List<BalanceHistoryRecord>> BalanceHistoryFromMonthsInPast(int monthsInPast, IEnumerable<int> includeBankAccounts)
        {
            var startDate = DateTime.Now.AddMonths(monthsInPast * -1);
            var endDate = DateTime.Now;

            return new List<BalanceHistoryRecord>
            {
                new BalanceHistoryRecord 
                {
                    BalanceHistory = await CalculateBalanceHistory(startDate, endDate, includeBankAccounts),
                }
            };
        }

        private async Task<List<decimal[]>> CalculateBalanceHistory(DateTime startDate, DateTime endDate, IEnumerable<int> bankAccountIds)
        {
            var userBankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

            // This might seem backwards, but this way we validate if the bankaccounts given are our own
            var bankAccounts = bankAccountIds.Any() ? userBankAccounts.Where(item => bankAccountIds.Any(y => y == item.Id)).ToList() : userBankAccounts;
            var bankAccountsIdFilter = bankAccounts.Select(x => x.Id).ToList();

            // Initialze the collection with a certain capacity to preserve ram usage
            var totalDays = (int)(endDate - startDate).TotalDays;
            var sumPerDates = new List<decimal[]>(totalDays + 1);

            decimal accountBalance = 0;
            using var unitOfWork = _unitOfWork();

            var transactions = (await unitOfWork.TransactionRepository.FindAll(x => bankAccountsIdFilter.Any(y => y == x.BankAccountId) &&
                                    x.Date >= startDate &&
                                    x.Date <= endDate))
                                    .OrderBy(item => item.Date)
                                    .ToList();

            accountBalance = bankAccounts.Sum(item => item.StartBalance);
            accountBalance += await unitOfWork.TransactionRepository.Sum(
                findQuery: x => bankAccountsIdFilter.Any(y => y == x.BankAccountId) && x.Date <= startDate,
                sumQuery: x => x.Amount);

            // Group by the date part, discard the time
            var transactionsPerDate = transactions.GroupBy(item => item.Date.Date).ToList();

            var firstDay = startDate.Date;
            var currentGroupingIndex = 0;
            for (int dayIndex = 0; dayIndex <= totalDays; dayIndex++)
            {
                var currentDay = firstDay.AddDays(dayIndex);

                if (currentGroupingIndex < transactionsPerDate.Count &&
                    transactionsPerDate[currentGroupingIndex].Key == currentDay)
                {
                    accountBalance = transactionsPerDate[currentGroupingIndex].Sum(item => item.Amount) + accountBalance;

                    sumPerDates.Add(new[]
                    {
                        Convert.ToDecimal(((currentDay - DateTimeOffset.UnixEpoch).TotalMilliseconds)),
                        accountBalance
                    });

                    currentGroupingIndex++;
                }
                else
                {
                    sumPerDates.Add(new[]
                    {
                        Convert.ToDecimal(((currentDay - DateTimeOffset.UnixEpoch).TotalMilliseconds)),
                        accountBalance
                    });
                }
            }

            /*foreach (var groupedTransactions in transactionsPerDate)
            {
                accountBalance = groupedTransactions.Sum(item => item.Amount) + accountBalance;

                sumPerDates.Add(new[]
                {
                    Convert.ToDecimal((groupedTransactions.Key - DateTimeOffset.UnixEpoch).TotalMilliseconds),
                    accountBalance
                });
            }*/

            return sumPerDates;
        }
    }
}
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class BalanceHistoryCalculation : IBalanceHistoryCalculation
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IBankAccountService _bankAccountService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public BalanceHistoryCalculation(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService,
            IBankAccountService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _bankAccountService = bankAccountService;
        }

        public async Task<List<decimal[]>> BalanceHistoryForYear(int year, IEnumerable<int> includeBankAccounts)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59, 999);

            return await CalculateBalanceHistory(startDate, endDate, includeBankAccounts);
        }

        public async Task<List<decimal[]>> BalanceHistoryFromYearInPast(int yearsInPast, IEnumerable<int> includeBankAccounts)
        {
            var startDate = DateTime.Now.AddYears(yearsInPast * -1);
            var endDate = DateTime.Now;

            return await CalculateBalanceHistory(startDate, endDate, includeBankAccounts);
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

            var userId = await _authenticationService.GetCurrentUserId();
            var transactions = (await unitOfWork.TransactionRepository.FindAll(x => bankAccountsIdFilter.Any(y => y == x.BankAccountId) &&
                                    x.UserId == userId &&
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

            // Add the beginning of the year transaction if there were previous transactions
            if (accountBalance > bankAccounts.Sum(item => item.StartBalance) && transactionsPerDate.First().Key.Month != 1 &&
                transactionsPerDate.First().Key.Day != 1)
            {
                sumPerDates.Add(new[]
                {
                    Convert.ToDecimal((startDate - new DateTimeOffset()).TotalMilliseconds),
                    accountBalance
                });
            }

            foreach (var groupedTransactions in transactionsPerDate)
            {
                accountBalance = groupedTransactions.Sum(item => item.Amount) + accountBalance;

                sumPerDates.Add(new[]
                {
                    Convert.ToDecimal((groupedTransactions.Key - new DateTimeOffset()).TotalMilliseconds),
                    accountBalance
                });
            }

            return sumPerDates;
        }
    }
}
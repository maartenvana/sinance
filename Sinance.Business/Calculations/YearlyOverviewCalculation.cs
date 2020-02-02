using Sinance.Business.Calculations.Subcalculations;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.StandardReport.Yearly;
using Sinance.Storage;
using System;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class YearlyOverviewCalculation : IYearlyOverviewCalculation
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;
        private readonly IBankAccountService _bankAccountService;

        public YearlyOverviewCalculation(Func<IUnitOfWork> unitOfWork,
            IBankAccountService bankAccountService,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _bankAccountService = bankAccountService;
            _authenticationService = authenticationService;
        }

        public async Task<YearlyOverviewModel> CalculateForYear(int year)
        {
            var result = new YearlyOverviewModel
            {
                Year = year
            };

            var startYearDate = new DateTime(year, 01, 01);
            var nextYearDate = new DateTime(year + 1, 01, 01);

            var currentUserId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var totalStartBalance = await BalanceCalculations.TotalBalanceBeforeDate(currentUserId, unitOfWork, startYearDate);
            var totalEndBalance = await BalanceCalculations.TotalBalanceBeforeDate(currentUserId, unitOfWork, nextYearDate);

            result.TotalBalance = new YearBalance(totalStartBalance, totalEndBalance);

            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();
            foreach (var bankAccount in bankAccounts)
            {
                var bankAccountStartBalance = await BalanceCalculations.TotalBalanceForBankAccountBeforeDate(currentUserId, unitOfWork, startYearDate, bankAccount);
                var bankAccountEndBalance = await BalanceCalculations.TotalBalanceForBankAccountBeforeDate(currentUserId, unitOfWork, nextYearDate, bankAccount);

                result.BalancePerBankAccount.Add(bankAccount, new YearBalance(bankAccountStartBalance, bankAccountEndBalance));
            }

            return result;
        }
    }
}

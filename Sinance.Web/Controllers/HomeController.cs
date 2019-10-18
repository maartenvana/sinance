using Sinance.Business.Services;
using Sinance.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Storage.Entities;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Transactions;

namespace Sinance.Controllers
{
    /// <summary>
    /// Home controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAuthenticationService _sessionService;
        private readonly ITransactionService _transactionService;

        public HomeController(
            IAuthenticationService sessionService,
            ITransactionService transactionService,
            IBankAccountService bankAccountService)
        {
            _sessionService = sessionService;
            _transactionService = transactionService;
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Index action of the home controller
        /// </summary>
        /// <returns>View containing the overview</returns>
        public async Task<IActionResult> Index()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();
            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForUser(currentUserId);

            var monthYearDate = DateTime.Now.AddMonths(-1);

            var transactions = await _transactionService.GetTransactionsForUserForMonth(currentUserId, monthYearDate.Year, monthYearDate.Month);

            // No need to sort this list, we loop through it by month numbers
            var totalProfitLossLastMonth = transactions.Where(x => bankAccounts.Single(y => y.Id == x.BankAccountId).IncludeInProfitLossGraph == true).Sum(x => x.Amount);

            var totalIncomeLastMonth = transactions.Where(x =>
                        (!x.Categories.Any() || x.Categories.Any(x => x.CategoryId != 69)) && // Cashflow
                        x.Amount > 0).Sum(x => x.Amount);

            var totalExpensesLastMonth = transactions.Where(x =>
                        (!x.Categories.Any() || x.Categories.Any(x => x.CategoryId != 69)) && // Cashflow
                        x.Amount < 0).Sum(x => x.Amount * -1);

            // Yes it's ascending cause we are looking for the lowest amount
            var topExpenses = transactions.Where(x =>
                    (!x.Categories.Any() || x.Categories.Any(x => x.CategoryId != 69)) && // Cashflow
                    x.Amount < 0)
                .OrderBy(x => x.Amount)
                .Take(15)
                .ToList();

            var dashboardModel = new DashboardModel
            {
                BankAccounts = bankAccounts,
                BiggestExpenses = topExpenses,
                LastMonthProfitLoss = totalProfitLossLastMonth,
                LastMonthExpenses = totalExpensesLastMonth,
                LastMonthIncome = totalIncomeLastMonth
            };

            return View(dashboardModel);
        }
    }
}
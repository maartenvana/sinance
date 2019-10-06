using Sinance.Business.Services;
using Sinance.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;
using Sinance.Domain.Entities;

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
        private readonly Func<IUnitOfWork> _unitOfWork;

        public HomeController(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService sessionService,
            IBankAccountService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Index action of the home controller
        /// </summary>
        /// <returns>View containing the overview</returns>
        public async Task<IActionResult> Index()
        {
            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();
            var currentUserId = await _sessionService.GetCurrentUserId();

            var monthYearDate = DateTime.Now.AddMonths(-1);

            // Yes, wow this is inneficient
            using var unitOfWork = _unitOfWork();

            var transactions = await unitOfWork.TransactionRepository.FindAll(findQuery: x =>
                        x.Date.Year == monthYearDate.Year &&
                        x.Date.Month == monthYearDate.Month &&
                        x.UserId == currentUserId,
                        includeProperties: nameof(Transaction.BankAccount));

            // No need to sort this list, we loop through it by month numbers
            var totalProfitLossLastMonth = transactions.Where(x => x.BankAccount.IncludeInProfitLossGraph == true).Sum(x => x.Amount);

            var totalIncomeLastMonth = transactions.Where(x =>
                        (!x.TransactionCategories.Any() || x.TransactionCategories.Any(x => x.CategoryId != 69)) && // Cashflow
                        x.Amount > 0).Sum(x => x.Amount);

            var totalExpensesLastMonth = transactions.Where(x =>
                        (!x.TransactionCategories.Any() || x.TransactionCategories.Any(x => x.CategoryId != 69)) && // Cashflow
                        x.Amount < 0).Sum(x => x.Amount * -1);

            // Yes it's ascending cause we are looking for the lowest amount
            var topExpenses = transactions.Where(x =>
                    (!x.TransactionCategories.Any() || x.TransactionCategories.Any(x => x.CategoryId != 69)) && // Cashflow
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
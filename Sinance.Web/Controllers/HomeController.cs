using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

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
            var bankAccounts = await this._bankAccountService.GetActiveBankAccountsForCurrentUser();
            var currentUserId = await this._sessionService.GetCurrentUserId();

            var monthYearDate = DateTime.Now.AddMonths(-1);

            // Yes, wow this is inneficient
            using (var unitOfWork = _unitOfWork())
            {
                // No need to sort this list, we loop through it by month numbers
                var totalProfitLossLastMonth = unitOfWork.TransactionRepository
                    .Sum(
                        filterPredicate: item =>
                            item.Date.Year == monthYearDate.Year &&
                            item.Date.Month == monthYearDate.Month &&
                            item.UserId == currentUserId,
                        sumPredicate: x => x.Amount);

                var totalIncomeLastMonth = unitOfWork.TransactionRepository
                    .Sum(
                        filterPredicate: item =>
                            item.Date.Year == monthYearDate.Year &&
                            item.Date.Month == monthYearDate.Month &&
                            item.UserId == currentUserId &&
                            (!item.TransactionCategories.Any() || item.TransactionCategories.Any(x => x.CategoryId != 69)) && // Cashflow
                            item.Amount > 0,
                        sumPredicate: x => x.Amount);

                var totalExpensesLastMonth = unitOfWork.TransactionRepository
                    .Sum(
                        filterPredicate: item =>
                            item.Date.Year == monthYearDate.Year &&
                            item.Date.Month == monthYearDate.Month &&
                            item.UserId == currentUserId &&
                            (!item.TransactionCategories.Any() || item.TransactionCategories.Any(x => x.CategoryId != 69)) && // Cashflow
                            item.Amount < 0,
                        sumPredicate: x => x.Amount * -1);

                // Yes it's ascending cause we are looking for the lowest amount
                var topExpenses = unitOfWork.TransactionRepository.FindTopAscending(item =>
                            item.Date.Year == monthYearDate.Year &&
                            item.Date.Month == monthYearDate.Month &&
                            item.UserId == currentUserId &&
                            (!item.TransactionCategories.Any() || item.TransactionCategories.Any(x => x.CategoryId != 69)) && // Cashflow
                            item.Amount < 0,
                            orderByAscending: x => x.Amount,
                            count: 15);

                DashboardModel dashboardModel = new DashboardModel
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
}
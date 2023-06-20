using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Constants;
using Sinance.Business.Services.BankAccounts;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.Transactions;
using Sinance.Web.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Controllers;

/// <summary>
/// Home controller
/// </summary>
[Authorize]
public class HomeController : Controller
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;

    public HomeController(
        ICategoryService categoryService,
        ITransactionService transactionService,
        IBankAccountService bankAccountService)
    {
        _categoryService = categoryService;
        _transactionService = transactionService;
        _bankAccountService = bankAccountService;
    }

    /// <summary>
    /// Index action of the home controller
    /// </summary>
    /// <returns>View containing the overview</returns>
    public async Task<IActionResult> Index()
    {
        var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

        var monthYearDate = DateTime.Now.AddMonths(-1);

        var transactions = await _transactionService.GetTransactionsForMonthForCurrentUser(monthYearDate.Year, monthYearDate.Month);

        var allCategories = await _categoryService.GetAllCategoriesForCurrentUser();
        var internalCashFlowCategory = allCategories.Single(x => x.Name == StandardCategoryNames.InternalCashFlowName);

        // No need to sort this list, we loop through it by month numbers
        var totalProfitLossLastMonth = transactions.Sum(x => x.Amount);

        var totalIncomeLastMonth = transactions.Where(x =>
                    (!x.Categories.Any() || x.Categories.Any(x => x.CategoryId != internalCashFlowCategory.Id)) && // Cashflow
                    x.Amount > 0).Sum(x => x.Amount);

        var totalExpensesLastMonth = transactions.Where(x =>
                    (!x.Categories.Any() || x.Categories.Any(x => x.CategoryId != internalCashFlowCategory.Id)) && // Cashflow
                    x.Amount < 0).Sum(x => x.Amount * -1);

        // Yes it's ascending cause we are looking for the lowest amount
        var topExpenses = transactions.Where(x =>
                (!x.Categories.Any() || x.Categories.Any(x => x.CategoryId != internalCashFlowCategory.Id)) && // Cashflow
                x.Amount < 0)
            .OrderBy(x => x.Amount)
            .Take(15)
            .ToList();

        var dashboardModel = new DashboardViewModel
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
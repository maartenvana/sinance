using Microsoft.EntityFrameworkCore;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.StandardReport.Income;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations;

public class IncomeCalculation : IIncomeCalculation
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;

    public IncomeCalculation(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<BiMonthlyIncomeReportModel> BiMonthlyIncomePerCategoryReport(DateTime startMonth)
    {
        var nextMonthStart = startMonth.AddMonths(1);
        var nextMonthEnd = nextMonthStart.AddMonths(1).AddDays(-1);

        using var context = _dbContextFactory.CreateDbContext();

        var transactions = await context.Transactions
            .Include(x => x.Category)
            .Where(item =>
                item.Date >= startMonth &&
                item.Date <= nextMonthEnd &&
                item.Amount > 0)
            .ToListAsync();

        var allCategories = await context.Categories
            .Include(x => x.ParentCategory)
            .Include(x => x.ChildCategories)
            .ToListAsync();

        var bimonthlyIncomeReport = new BimonthlyIncomeReportItem
        {
            Incomes = new List<BimonthlyIncome>(),
            FirstMonth = startMonth,
            SecondMonth = nextMonthStart
        };

        // loop through all the regular top categories
        foreach (var parentCategory in allCategories.Where(category =>
            category.ParentId == null))
        {
            AddCategoryToBimonthlyIncome(transactions, parentCategory, bimonthlyIncomeReport, startMonth, nextMonthStart);
        }

        var uncategorizedTransactions = transactions.Where(item =>
            item.CategoryId == null &&
            item.Date >= startMonth &&
            item.Date <= nextMonthEnd).ToList().ToDto();

        return new BiMonthlyIncomeReportModel
        {
            BimonthlyIncomeReport = bimonthlyIncomeReport,
            UncategorizedTransactions = uncategorizedTransactions
        };
    }

    private static void AddCategoryToBimonthlyIncome(IList<TransactionEntity> transactions, CategoryEntity category,
        BimonthlyIncomeReportItem bimonthlyExpenseReport, DateTime firstMonthStart, DateTime secondMonthStart)
    {
        var lastMonthParentTransactions = TransactionsForMonth(transactions, category, firstMonthStart.Year, firstMonthStart.Month);
        var thisMonthParentTransactions = TransactionsForMonth(transactions, category, secondMonthStart.Year, secondMonthStart.Month);

        var bimonthlyParentIncome = new BimonthlyIncome
        {
            Name = category.Name,
            AmountPrevious = lastMonthParentTransactions.Sum(x => x.Amount),
            AmountNow = thisMonthParentTransactions.Sum(x => x.Amount)
        };

        bimonthlyExpenseReport.ThisMonthTotal += bimonthlyParentIncome.AmountNow;
        bimonthlyExpenseReport.PreviousMonthTotal += bimonthlyParentIncome.AmountPrevious;

        bimonthlyExpenseReport.Incomes.Add(bimonthlyParentIncome);

        if (category.ChildCategories.Any())
        {
            foreach (var childCategory in category.ChildCategories)
            {
                var lastMonthChildTransactions = TransactionsForMonth(transactions, childCategory, firstMonthStart.Year, firstMonthStart.Month);
                var thisMonthChildTransactions = TransactionsForMonth(transactions, childCategory, secondMonthStart.Year, secondMonthStart.Month);

                var bimonthlyChildIncome = new BimonthlyIncome
                {
                    Name = childCategory.Name,
                    AmountPrevious = lastMonthChildTransactions.Sum(x => x.Amount),
                    AmountNow = thisMonthChildTransactions.Sum(x => x.Amount)
                };

                bimonthlyExpenseReport.ThisMonthTotal += bimonthlyChildIncome.AmountNow;
                bimonthlyExpenseReport.PreviousMonthTotal += bimonthlyChildIncome.AmountPrevious;

                bimonthlyParentIncome.AmountNow += bimonthlyChildIncome.AmountNow;
                bimonthlyParentIncome.AmountPrevious += bimonthlyChildIncome.AmountPrevious;

                bimonthlyParentIncome.ChildBimonthlyIncomes.Add(bimonthlyChildIncome);
            }
        }
    }

    /// <summary>
    /// Searches for transactions between two dates and that are mapped to the given category
    /// </summary>
    /// <param name="transactions">Transactions to search</param>
    /// <param name="category">Category to search for</param>
    /// <param name="monthStart">Transactions need to occur after this date</param>
    /// <param name="nextMonthStart">Transactions need to occur before this date</param>
    /// <returns>List of matching transactions</returns>
    private static IList<TransactionEntity> TransactionsForMonth(IList<TransactionEntity> transactions, CategoryEntity category, int year, int month)
    {
        var lastMonthParentTransactions = transactions.Where(
                item =>
                    item.Date.Year == year &&
                    item.Date.Month == month &&
                    item.CategoryId == category.Id).ToList();

        return lastMonthParentTransactions;
    }
}
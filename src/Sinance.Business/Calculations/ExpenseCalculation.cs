using Microsoft.EntityFrameworkCore;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.StandardReport.Expense;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations;

public class ExpenseCalculation : IExpenseCalculation
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;

    public ExpenseCalculation(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<BiMonthlyExpenseReportModel> BiMonthlyExpensePerCategoryReport(DateTime startMonth)
    {
        var nextMonthStart = startMonth.AddMonths(1);
        var nextMonthEnd = nextMonthStart.AddMonths(1).AddDays(-1);

        using var context = _dbContextFactory.CreateDbContext();

        var transactions = await context.Transactions.Where(item =>
                       item.Date >= startMonth &&
                                      item.Date <= nextMonthEnd &&
                                                     item.Amount < 0)
            .Include(x => x.Category)
            .ToListAsync();

        var allCategories = await context.Categories
            .Include(x => x.ParentCategory)
            .Include(x => x.ChildCategories)
            .ToListAsync();

        var regularBimonthlyExpenseReport = new BimonthlyExpenseReportItem
        {
            Expenses = new List<BimonthlyExpense>(),
            FirstMonthDate = startMonth,
            SecondMonthDate = nextMonthStart
        };
        var volatileMonthlyExpenseReport = new BimonthlyExpenseReportItem
        {
            Expenses = new List<BimonthlyExpense>(),
            FirstMonthDate = startMonth,
            SecondMonthDate = nextMonthStart
        };

        // loop through all the regular top categories
        foreach (var parentCategory in allCategories.Where(category =>
            category.ParentId == null &&
            (category.IsRegular || category.ChildCategories.Any(childCategory => childCategory.IsRegular))))
        {
            AddCategoryToBimonthlyExpense(transactions, parentCategory, regularBimonthlyExpenseReport, startMonth, nextMonthStart, true);
        }

        // loop through all the volatile top categories
        foreach (var parentCategory in allCategories.Where(category =>
            category.ParentId == null &&
            (!category.IsRegular || category.ChildCategories.Any(childCategory =>
                !childCategory.IsRegular &&
                (childCategory.ParentCategory != null && !childCategory.ParentCategory.IsRegular)))))
        {
            AddCategoryToBimonthlyExpense(transactions, parentCategory, volatileMonthlyExpenseReport, startMonth, nextMonthStart, false);
        }

        var uncategorizedTransactions = transactions.Where(item =>
            item.CategoryId == null &&
            item.Date >= startMonth &&
            item.Date <= nextMonthEnd).ToList().ToDto();

        return new BiMonthlyExpenseReportModel
        {
            RegularBimonthlyExpenseReport = regularBimonthlyExpenseReport,
            VolatileBimonthlyExpenseReport = volatileMonthlyExpenseReport,
            UncategorizedTransactions = uncategorizedTransactions
        };
    }

    public async Task<Dictionary<string, Dictionary<int, decimal>>> ExpensePerCategoryIdPerMonthForYear(int year, IEnumerable<int> categoryIds)
    {
        var dateRangeStart = new DateTime(year, 1, 1);
        var dateRangeEnd = new DateTime(year, 12, 31);

        using var context = _dbContextFactory.CreateDbContext();

        var transactions = await context.Transactions.Where(item =>
            item.Date >= dateRangeStart &&
            item.Date <= dateRangeEnd &&
            item.Amount < 0 &&
            categoryIds.Any(reportCategory => reportCategory == item.CategoryId))
            .Include(x => x.Category)
            .ToListAsync();

        var categories = await context.Categories.Where(x => categoryIds.Any(y => y == x.Id))
            .Include(x => x.ParentCategory)
            .Include(x => x.ChildCategories)
            .ToListAsync();

        var reportDictionary = categories.ToDictionary(
            keySelector: x => CreateCategoryNameWithOptionalParent(x),
            elementSelector: x => CreateMonthlyDictionary());

        foreach (var transaction in transactions)
        {
            if (transaction.Category != null)
            {
                var amount = GetPositiveAmount(transaction.Amount);

                reportDictionary[CreateCategoryNameWithOptionalParent(transaction.Category)][transaction.Date.Month] += amount;
            }
            else
            {
                const string noCategoryName = "Geen categorie";
                if (!reportDictionary.ContainsKey(noCategoryName))
                {
                    reportDictionary.Add(noCategoryName, CreateMonthlyDictionary());
                }

                // Make sure the number is positive
                reportDictionary[noCategoryName][transaction.Date.Month] += GetPositiveAmount(transaction.Amount);
            }
        }

        return reportDictionary;
    }

    private static void AddCategoryToBimonthlyExpense(IList<TransactionEntity> transactions, CategoryEntity category,
                        BimonthlyExpenseReportItem bimonthlyExpenseReport, DateTime firstMonthStart, DateTime nextMonthStart, bool regularExpense)
    {
        var firstMonthParentTransactions = TransactionsForMonth(transactions, category, firstMonthStart.Year, firstMonthStart.Month);
        var secondMonthParentTransactions = TransactionsForMonth(transactions, category, nextMonthStart.Year, nextMonthStart.Month);

        var bimonthlyParentExpense = new BimonthlyExpense
        {
            Name = category.Name,
            AmountPrevious = firstMonthParentTransactions.Sum(x => x.Amount),
            AmountNow = secondMonthParentTransactions.Sum(x => x.Amount)
        };

        if (bimonthlyParentExpense.AmountNow != bimonthlyParentExpense.AmountPrevious &&
            secondMonthParentTransactions.Count < firstMonthParentTransactions.Count)
        {
            bimonthlyExpenseReport.RemainingAmount -= bimonthlyParentExpense.AmountNow - bimonthlyParentExpense.AmountPrevious;
        }

        bimonthlyExpenseReport.ThisMonthTotal += bimonthlyParentExpense.AmountNow;
        bimonthlyExpenseReport.PreviousMonthTotal += bimonthlyParentExpense.AmountPrevious;

        bimonthlyExpenseReport.Expenses.Add(bimonthlyParentExpense);

        if (category.ChildCategories.Any())
        {
            foreach (var childCategory in category.ChildCategories.Where(item => item.IsRegular == regularExpense || category.IsRegular == regularExpense))
            {
                var lastMonthChildTransactions = TransactionsForMonth(transactions, childCategory, firstMonthStart.Year, firstMonthStart.Month);
                var thisMonthChildTransactions = TransactionsForMonth(transactions, childCategory, nextMonthStart.Year, nextMonthStart.Month);

                var bimonthlyChildExpense = new BimonthlyExpense
                {
                    Name = childCategory.Name,
                    AmountPrevious = lastMonthChildTransactions.Sum(x => x.Amount),
                    AmountNow = thisMonthChildTransactions.Sum(x => x.Amount),
                };

                if (bimonthlyChildExpense.AmountNow != bimonthlyChildExpense.AmountPrevious &&
                    thisMonthChildTransactions.Count < lastMonthChildTransactions.Count)
                {
                    bimonthlyExpenseReport.RemainingAmount -= bimonthlyChildExpense.AmountNow - bimonthlyChildExpense.AmountPrevious;
                }

                bimonthlyExpenseReport.ThisMonthTotal += bimonthlyChildExpense.AmountNow;
                bimonthlyExpenseReport.PreviousMonthTotal += bimonthlyChildExpense.AmountPrevious;

                bimonthlyParentExpense.AmountNow += bimonthlyChildExpense.AmountNow;
                bimonthlyParentExpense.AmountPrevious += bimonthlyChildExpense.AmountPrevious;

                bimonthlyParentExpense.ChildBimonthlyExpenses.Add(bimonthlyChildExpense);
            }
        }
    }

    private static string CreateCategoryNameWithOptionalParent(CategoryEntity category)
    {
        if (category.ParentCategory != null)
        {
            return $"({category.ParentCategory.Name}) {category.Name}";
        }
        else
        {
            return category.Name;
        }
    }

    private static Dictionary<int, decimal> CreateMonthlyDictionary() =>
                new()
                {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 },
            { 6, 0 },
            { 7, 0 },
            { 8, 0 },
            { 9, 0 },
            { 10, 0 },
            { 11, 0 },
            { 12, 0 }
                };

    private static decimal GetPositiveAmount(decimal amount)
    {
        return amount < 0 ? amount * -1 : amount;
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
        IList<TransactionEntity> lastMonthParentTransactions = transactions.Where(
                item =>
                    item.Date.Year == year &&
                    item.Date.Month == month &&
                    item.CategoryId == category.Id).ToList();
        return lastMonthParentTransactions;
    }
}
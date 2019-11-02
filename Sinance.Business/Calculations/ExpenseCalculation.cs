using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.StandardReport.Expense;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class ExpenseCalculation : IExpenseCalculation
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ExpenseCalculation(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<BiMonthlyExpenseReportModel> BiMonthlyExpensePerCategoryReport(DateTime startMonth)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            var nextMonthStart = startMonth.AddMonths(1);
            var nextMonthEnd = nextMonthStart.AddMonths(1).AddDays(-1);

            using var unitOfWork = _unitOfWork();

            var transactions = await unitOfWork.TransactionRepository.FindAll(
                 findQuery: item =>
                     item.Date >= startMonth &&
                     item.Date <= nextMonthEnd &&
                     item.UserId == userId &&
                     item.Amount < 0,
                 includeProperties: nameof(TransactionEntity.TransactionCategories));

            var allCategories = await unitOfWork.CategoryRepository.FindAll(
                findQuery: item => item.UserId == userId,
                includeProperties: new string[] {
                    nameof(CategoryEntity.ParentCategory),
                    nameof(CategoryEntity.ChildCategories)
                });

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
                item.TransactionCategories.Count == 0 &&
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

            using var unitOfWork = _unitOfWork();
            var userId = await _authenticationService.GetCurrentUserId();

            var transactions = await unitOfWork.TransactionRepository.FindAll(
                    findQuery: item =>
                        item.Date >= dateRangeStart &&
                        item.Date <= dateRangeEnd &&
                        item.UserId == userId &&
                        item.Amount < 0 &&
                        item.TransactionCategories.Any(transactionCategory =>
                            categoryIds.Any(reportCategory =>
                                reportCategory == transactionCategory.CategoryId)),
                    includeProperties: new string[] {
                        nameof(TransactionEntity.TransactionCategories)
                    });

            var categories = await unitOfWork.CategoryRepository.FindAll(x => categoryIds.Any(y => y == x.Id), includeProperties: new string[] { nameof(CategoryEntity.ParentCategory) });

            var reportDictionary = categories.ToDictionary(
                keySelector: x => CreateCategoryNameWithOptionalParent(x),
                elementSelector: x => CreateMonthlyDictionary());

            foreach (var transaction in transactions)
            {
                if (transaction.TransactionCategories?.Any() == true)
                {
                    foreach (var transactionCategory in transaction.TransactionCategories.Where(transactionCategory => categoryIds.Any(reportCategory => reportCategory == transactionCategory.CategoryId)))
                    {
                        var category = categories.Single(x => x.Id == transactionCategory.CategoryId);

                        var amount = transactionCategory.Amount ?? transaction.Amount;
                        amount = GetPositiveAmount(amount);

                        reportDictionary[CreateCategoryNameWithOptionalParent(category)][transaction.Date.Month] += amount;
                    }
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
                AmountPrevious = CalculateSumCategoryTransactions(category, firstMonthParentTransactions),
                AmountNow = CalculateSumCategoryTransactions(category, secondMonthParentTransactions)
            };

            if (bimonthlyParentExpense.AmountNow != bimonthlyParentExpense.AmountPrevious &&
                secondMonthParentTransactions.Count() < firstMonthParentTransactions.Count())
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
                        AmountPrevious = CalculateSumCategoryTransactions(childCategory, lastMonthChildTransactions),
                        AmountNow = CalculateSumCategoryTransactions(childCategory, thisMonthChildTransactions),
                    };

                    if (bimonthlyChildExpense.AmountNow != bimonthlyChildExpense.AmountPrevious &&
                        thisMonthChildTransactions.Count() < lastMonthChildTransactions.Count())
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

        /// <summary>
        /// Calculates the sum for each transactions for the given category
        ///
        /// if the transactions is split up in different categories then take the amount of the split
        /// if the transactions is not split up take the full amount
        /// </summary>
        /// <param name="category">Category to look for</param>
        /// <param name="transactions">Transactions to use</param>
        /// <returns></returns>
        private static decimal CalculateSumCategoryTransactions(CategoryEntity category, IList<TransactionEntity> transactions)
        {
            return transactions.Sum(item => item.TransactionCategories.Any(transCategory => transCategory.Amount == null) ?
                                        item.Amount :
                                        item.TransactionCategories.Where(transCategory => transCategory.CategoryId == category.Id)
                                            .Sum(transCategory => transCategory.Amount.GetValueOrDefault()));
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
                    new Dictionary<int, decimal>
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
                        item.TransactionCategories.Any(transactionCategory =>
                            transactionCategory.CategoryId == category.Id)).ToList();
            return lastMonthParentTransactions;
        }
    }
}
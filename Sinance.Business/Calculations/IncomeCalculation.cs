using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.StandardReport.Income;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class IncomeCalculation : IIncomeCalculation
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public IncomeCalculation(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<BiMonthlyIncomeReportModel> BiMonthlyIncomePerCategoryReport(DateTime startMonth)
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
                    item.Amount > 0,
                includeProperties: nameof(TransactionEntity.TransactionCategories));

            var allCategories = await unitOfWork.CategoryRepository.FindAll(
                findQuery: item => item.UserId == userId,
                includeProperties: new string[]
                {
                    nameof(CategoryEntity.ParentCategory),
                    nameof(CategoryEntity.ChildCategories)
                });

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

            var uncategorizedTransactionsThisMonth = transactions.Where(item =>
                item.TransactionCategories.Count == 0 &&
                item.Date < nextMonthStart &&
                item.Date >= startMonth).ToList().ToDto();

            return new BiMonthlyIncomeReportModel
            {
                BimonthlyIncomeReport = bimonthlyIncomeReport,
                UncategorizedTransactionsThisMonth = uncategorizedTransactionsThisMonth
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
                AmountPrevious = CalculateSumCategoryTransactions(category, lastMonthParentTransactions),
                AmountNow = CalculateSumCategoryTransactions(category, thisMonthParentTransactions)
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
                        AmountPrevious = CalculateSumCategoryTransactions(childCategory, lastMonthChildTransactions),
                        AmountNow = CalculateSumCategoryTransactions(childCategory, thisMonthChildTransactions),
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
                        item.TransactionCategories.Any(transactionCategory =>
                            transactionCategory.CategoryId == category.Id)).ToList();
            return lastMonthParentTransactions;
        }
    }
}
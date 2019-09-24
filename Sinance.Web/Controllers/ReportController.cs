using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Controllers
{
    /// <summary>
    /// Controller for all reports
    /// </summary>
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ReportController(
            IAuthenticationService sessionService,
            Func<IUnitOfWork> unitOfWork)
        {
            _sessionService = sessionService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Action for displaying the add page for a custom graph report
        /// </summary>
        /// <returns>Add page for a custom graph report</returns>
        public async Task<IActionResult> AddCustomGraphReport()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                List<Category> allCategories = unitOfWork.CategoryRepository.FindAll(item => item.UserId == currentUserId).ToList();

                List<BasicCheckBoxItem> availableCategories = new List<BasicCheckBoxItem>();

                availableCategories.AddRange(allCategories.ConvertAll(item => new BasicCheckBoxItem
                {
                    Id = item.Id,
                    Name = item.Name,
                }));

                return View("UpsertCustomGraphReport", new CustomGraphReportSettingsModel
                {
                    AvailableCategories = availableCategories
                });
            }
        }

        /// <summary>
        /// Displays the Custom report page
        /// </summary>
        /// <param name="reportId">Identifier of the custom report</param>
        /// <returns>A custom report page</returns>
        public async Task<IActionResult> CustomReport(int reportId)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                CustomReport report =
                unitOfWork.CustomReportRepository.FindSingle(item => item.Id == reportId && item.UserId == currentUserId);

                return View("CustomReport", report);
            }
        }

        /// <summary>
        /// Edit action for custom reports
        /// </summary>
        /// <param name="reportId">Identifier of the report to edit</param>
        /// <returns>View with the custom report to edit</returns>
        public async Task<IActionResult> EditCustomGraphReport(int reportId)
        {
            ActionResult result = null;

            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                List<Category> allCategories = unitOfWork.CategoryRepository.FindAll(item => item.UserId == currentUserId).ToList();

                CustomReport customReport = unitOfWork.CustomReportRepository.FindSingle(item => item.Id == reportId && item.UserId == currentUserId, "ReportCategories");

                List<BasicCheckBoxItem> availableCategories = new List<BasicCheckBoxItem>();

                availableCategories.AddRange(allCategories.ConvertAll(category => new BasicCheckBoxItem
                {
                    Id = category.Id,
                    Name = category.Name,
                    Checked = customReport.ReportCategories.Any(reportCategory => reportCategory.Id == category.Id)
                }));

                if (customReport != null)
                {
                    result = View("UpsertCustomGraphReport", new CustomGraphReportSettingsModel
                    {
                        AvailableCategories = availableCategories,
                        Id = reportId,
                        Name = customReport.Name
                    });
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(reportId), Resources.NoReportFound);

                return result;
            }
        }

        /// <summary>
        /// Action for displaying all the expenses per category
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Fine to maintain")]
        public async Task<IActionResult> ExpenseOverview()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            // E.G. 01-02-2014
            DateTime previousMonthStart = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
            // E.G. 01-03-2014
            DateTime thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            // E.G. 01-04-2014
            DateTime nextMonthStart = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1);

            using (var unitOfWork = _unitOfWork())
            {
                IList<Transaction> transactions =
                unitOfWork.TransactionRepository.FindAll(item => item.Date >= previousMonthStart && item.Date < nextMonthStart &&
                    item.UserId == currentUserId && item.Amount < 0,
                    includeProperties: "TransactionCategories").ToList();
                IEnumerable<Category> allCategories = unitOfWork.CategoryRepository.FindAll(item => item.UserId == currentUserId, "ParentCategory", "ChildCategories");

                BimonthlyExpenseReport regularBimonthlyExpenseReport = new BimonthlyExpenseReport
                {
                    Expenses = new List<BimonthlyExpense>(),
                    PreviousMonthDate = previousMonthStart,
                    ThisMonthDate = thisMonthStart
                };
                BimonthlyExpenseReport volatileMonthlyExpenseReport = new BimonthlyExpenseReport
                {
                    Expenses = new List<BimonthlyExpense>(),
                    PreviousMonthDate = previousMonthStart,
                    ThisMonthDate = thisMonthStart
                };

                // loop through all the regular top categories
                foreach (Category parentCategory in allCategories.Where(category =>
                    category.ParentId == null &&
                    (category.IsRegular || category.ChildCategories.Any(childCategory => childCategory.IsRegular))))
                {
                    AddCategoryToBimonthlyExpense(transactions, parentCategory, regularBimonthlyExpenseReport, previousMonthStart, thisMonthStart, nextMonthStart, true);
                }

                // loop through all the volatile top categories
                foreach (Category parentCategory in allCategories.Where(category =>
                    category.ParentId == null &&
                    (!category.IsRegular || category.ChildCategories.Any(childCategory =>
                        !childCategory.IsRegular &&
                        (childCategory.ParentCategory != null && !childCategory.ParentCategory.IsRegular)))))
                {
                    AddCategoryToBimonthlyExpense(transactions, parentCategory, volatileMonthlyExpenseReport, previousMonthStart, thisMonthStart, nextMonthStart, false);
                }

                IList<Transaction> uncategorizedTransactionsThisMonth = transactions.Where(item =>
                    item.TransactionCategories.Count == 0 &&
                    item.Date < nextMonthStart &&
                    item.Date >= thisMonthStart).ToList();

                ExpensesModel expensesModel = new ExpensesModel
                {
                    RegularBimonthlyExpenseReport = regularBimonthlyExpenseReport,
                    VolatileBimonthlyExpenseReport = volatileMonthlyExpenseReport,
                    UncategorizedTransactionsThisMonth = uncategorizedTransactionsThisMonth
                };

                return View("ExpenseOverview", expensesModel);
            }
        }

        /// <summary>
        /// Expense per category report page
        /// </summary>
        /// <returns>Expense per category report</returns>
        public async Task<IActionResult> ExpensePerCategory()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                var categories = unitOfWork.CategoryRepository.FindAll(item => item.UserId == currentUserId);
                ExpensePerCategoryModel model = new ExpensePerCategoryModel();

                if (categories != null)
                {
                    model.AvailableCategories = categories;
                    model.TimeFilter = TimeFilter.Month;
                    model.SelectedCategoryId = categories.First().Id;
                    model.SelectedMonth = DateTime.Now.Month.ToString(CultureInfo.CurrentCulture);
                    model.SelectedYear = DateTime.Now.Year.ToString(CultureInfo.CurrentCulture);
                }

                return View(model);
            }
        }

        /// <summary>
        /// Partial view containing the report with expenses per category
        /// </summary>
        /// <param name="model">Model containing the parameters for the report</param>
        /// <returns>Report of expenses within a category</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public ActionResult ExpensePerCategoryPartial(ExpensePerCategoryModel model)
        {
            using (var unitOfWork = _unitOfWork())
            {
                Category category = unitOfWork.CategoryRepository.FindSingle(item => item.Id == model.SelectedCategoryId);

                IEnumerable<Transaction> transactions = category.TransactionCategories.Where(
                    item => item.Transaction.Date.Month == int.Parse(model.SelectedMonth, CultureInfo.CurrentCulture) &&
                    item.Transaction.Date.Year == int.Parse(model.SelectedYear, CultureInfo.CurrentCulture) &&
                    item.Transaction.Amount < 0).Select(item => item.Transaction);

                DateTime utcStartDate = new DateTime(1970, 1, 1);
                List<IGrouping<DateTime, Transaction>> transactionsPerDate = transactions.GroupBy(item => item.Date).ToList();

                IList<decimal[]> sumPerDate = new List<decimal[]>();

                foreach (IGrouping<DateTime, Transaction> groupedTransactions in transactionsPerDate)
                {
                    sumPerDate.Add(new[]
                    {
                    Convert.ToDecimal(groupedTransactions.Key.Subtract(utcStartDate).TotalMilliseconds),
                    groupedTransactions.Sum(item => item.Amount * -1)
                });
                }

                return PartialView("ExpensePerCategoryPartial", new ExpensesPerCategoryPartialModel
                {
                    CategoryName = category.Name,
                    Transactions = transactions.OrderByDescending(item => item.Date).ThenBy(item => item.BankAccountId).ToList(),
                    GraphValues = sumPerDate
                });
            }
        }

        /// <summary>
        /// Action for displaying all the income per category
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public async Task<IActionResult> IncomeOverview()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            // E.G. 01-02-2014
            DateTime previousMonthStart = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
            // E.G. 01-03-2014
            DateTime thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            // E.G. 01-04-2014
            DateTime nextMonthStart = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1);

            using (var unitOfWork = _unitOfWork())
            {
                IList<Transaction> transactions =
                unitOfWork.TransactionRepository.FindAll(item => item.Date >= previousMonthStart && item.Date < nextMonthStart &&
                    item.UserId == currentUserId && item.Amount > 0,
                    includeProperties: "TransactionCategories").ToList();
                IEnumerable<Category> allCategories = unitOfWork.CategoryRepository.FindAll(item => item.UserId == currentUserId, "ParentCategory", "ChildCategories");

                BimonthlyIncomeReport bimonthlyIncomeReport = new BimonthlyIncomeReport
                {
                    Incomes = new List<BimonthlyIncome>(),
                    PreviousMonthDate = previousMonthStart,
                    ThisMonthDate = thisMonthStart
                };

                // loop through all the regular top categories
                foreach (Category parentCategory in allCategories.Where(category =>
                    category.ParentId == null))
                {
                    AddCategoryToBimonthlyIncome(transactions, parentCategory, bimonthlyIncomeReport, previousMonthStart, thisMonthStart, nextMonthStart);
                }

                IList<Transaction> uncategorizedTransactionsThisMonth = transactions.Where(item =>
                    item.TransactionCategories.Count == 0 &&
                    item.Date < nextMonthStart &&
                    item.Date >= thisMonthStart).ToList();

                IncomeModel incomeModel = new IncomeModel
                {
                    BimonthlyIncomeReport = bimonthlyIncomeReport,
                    UncategorizedTransactionsThisMonth = uncategorizedTransactionsThisMonth
                };

                return View("IncomeOverview", incomeModel);
            }
        }

        /// <summary>
        /// Upserts a custom report to the database
        /// </summary>
        /// <param name="model">Model to upsert</param>
        /// <returns>Redirect to the upserted custom report</returns>
        public async Task<IActionResult> UpsertCustomGraphReport(CustomGraphReportSettingsModel model)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            CustomReport customReport = null;

            if (ModelState.IsValid)
            {
                using (var unitOfWork = _unitOfWork())
                {
                    if (model.Id <= 0)
                    {
                        customReport = new CustomReport
                        {
                            Name = model.Name,
                            UserId = currentUserId
                        };
                        unitOfWork.CustomReportRepository.Insert(customReport);
                        await unitOfWork.SaveAsync();
                    }
                    else
                        customReport = unitOfWork.CustomReportRepository.FindSingle(item => item.UserId == currentUserId && item.Id == model.Id);

                    if (customReport != null)
                    {
                        unitOfWork.CustomReportCategoryRepository.DeleteRange(unitOfWork.CustomReportCategoryRepository.FindAll(item => item.CustomReportId == customReport.Id));
                        await unitOfWork.SaveAsync();

                        List<BasicCheckBoxItem> selectedCategories = model.AvailableCategories.Where(item => item.Checked).ToList();

                        foreach (BasicCheckBoxItem selectedCategory in selectedCategories)
                        {
                            unitOfWork.CustomReportCategoryRepository.Insert(new CustomReportCategory
                            {
                                CategoryId = selectedCategory.Id,
                                CustomReportId = customReport.Id
                            });
                        }

                        await unitOfWork.SaveAsync();
                    }
                }
            }
            else
                throw new ArgumentException("Upsert of custom report failed");

            return RedirectToAction("CustomReport", new { reportId = customReport?.Id ?? model.Id });
        }

        /// <summary>
        /// Adds a category and its child categories to the given bimontly expense report
        /// </summary>
        /// <param name="transactions">transactions to search inside of</param>
        /// <param name="category">Category to add</param>
        /// <param name="bimonthlyExpenseReport">Report to update</param>
        /// <param name="previousMonthStart">When the previous month started</param>
        /// <param name="thisMonthStart">When this month started</param>
        /// <param name="nextMonthStart">When the next month is starting</param>
        /// <param name="regularExpense">If we are adding regular expenses or not</param>
        private static void AddCategoryToBimonthlyExpense(IList<Transaction> transactions, Category category,
            BimonthlyExpenseReport bimonthlyExpenseReport, DateTime previousMonthStart, DateTime thisMonthStart, DateTime nextMonthStart, bool regularExpense)
        {
            IList<Transaction> lastMonthParentTransactions = TransactionsForMonth(transactions, category, previousMonthStart, thisMonthStart);
            IList<Transaction> thisMonthParentTransactions = TransactionsForMonth(transactions, category, thisMonthStart, nextMonthStart);

            BimonthlyExpense bimonthlyParentExpense = new BimonthlyExpense
            {
                Name = category.Name,
                AmountPrevious = CalculateSumCategoryTransactions(category, lastMonthParentTransactions),
                AmountNow = CalculateSumCategoryTransactions(category, thisMonthParentTransactions)
            };

            if (bimonthlyParentExpense.AmountNow != bimonthlyParentExpense.AmountPrevious &&
                thisMonthParentTransactions.Count() < lastMonthParentTransactions.Count())
                bimonthlyExpenseReport.RemainingAmount -= bimonthlyParentExpense.AmountNow - bimonthlyParentExpense.AmountPrevious;

            bimonthlyExpenseReport.ThisMonthTotal += bimonthlyParentExpense.AmountNow;
            bimonthlyExpenseReport.PreviousMonthTotal += bimonthlyParentExpense.AmountPrevious;

            bimonthlyExpenseReport.Expenses.Add(bimonthlyParentExpense);

            if (category.ChildCategories.Any())
            {
                foreach (Category childCategory in category.ChildCategories.Where(item => item.IsRegular == regularExpense || category.IsRegular == regularExpense))
                {
                    IList<Transaction> lastMonthChildTransactions = TransactionsForMonth(transactions, childCategory, previousMonthStart, thisMonthStart);
                    IList<Transaction> thisMonthChildTransactions = TransactionsForMonth(transactions, childCategory, thisMonthStart, nextMonthStart);

                    BimonthlyExpense bimonthlyChildExpense = new BimonthlyExpense
                    {
                        Name = childCategory.Name,
                        AmountPrevious = CalculateSumCategoryTransactions(childCategory, lastMonthChildTransactions),
                        AmountNow = CalculateSumCategoryTransactions(childCategory, thisMonthChildTransactions),
                    };

                    if (bimonthlyChildExpense.AmountNow != bimonthlyChildExpense.AmountPrevious &&
                        thisMonthChildTransactions.Count() < lastMonthChildTransactions.Count())
                        bimonthlyExpenseReport.RemainingAmount -= bimonthlyChildExpense.AmountNow - bimonthlyChildExpense.AmountPrevious;

                    bimonthlyExpenseReport.ThisMonthTotal += bimonthlyChildExpense.AmountNow;
                    bimonthlyExpenseReport.PreviousMonthTotal += bimonthlyChildExpense.AmountPrevious;

                    bimonthlyParentExpense.AmountNow += bimonthlyChildExpense.AmountNow;
                    bimonthlyParentExpense.AmountPrevious += bimonthlyChildExpense.AmountPrevious;

                    bimonthlyParentExpense.ChildBimonthlyExpenses.Add(bimonthlyChildExpense);
                }
            }
        }

        private static void AddCategoryToBimonthlyIncome(IList<Transaction> transactions, Category category,
            BimonthlyIncomeReport bimonthlyExpenseReport, DateTime previousMonthStart, DateTime thisMonthStart, DateTime nextMonthStart)
        {
            IList<Transaction> lastMonthParentTransactions = TransactionsForMonth(transactions, category, previousMonthStart, thisMonthStart);
            IList<Transaction> thisMonthParentTransactions = TransactionsForMonth(transactions, category, thisMonthStart, nextMonthStart);

            BimonthlyIncome bimonthlyParentIncome = new BimonthlyIncome
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
                foreach (Category childCategory in category.ChildCategories)
                {
                    IList<Transaction> lastMonthChildTransactions = TransactionsForMonth(transactions, childCategory, previousMonthStart, thisMonthStart);
                    IList<Transaction> thisMonthChildTransactions = TransactionsForMonth(transactions, childCategory, thisMonthStart, nextMonthStart);

                    BimonthlyIncome bimonthlyChildIncome = new BimonthlyIncome
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
        private static decimal CalculateSumCategoryTransactions(Category category, IList<Transaction> transactions)
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
        private static IList<Transaction> TransactionsForMonth(IList<Transaction> transactions, Category category, DateTime monthStart, DateTime nextMonthStart)
        {
            IList<Transaction> lastMonthParentTransactions = transactions.Where(
                    item =>
                        item.Date >= monthStart &&
                        item.Date < nextMonthStart &&
                        item.TransactionCategories.Any(transactionCategory =>
                            transactionCategory.CategoryId == category.Id)).ToList();
            return lastMonthParentTransactions;
        }
    }
}
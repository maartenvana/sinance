using Sinance.Business.Classes;
using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Controllers
{
    /// <summary>
    /// Controller for all graph actions
    /// </summary>
    [Authorize]
    public class GraphController : Controller
    {
        private readonly IBankAccountService _bankAccountService;

        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        /// <summary>
        /// UTC start date for calculations
        /// </summary>
        private readonly DateTime _utcStartDate = new DateTime(1970, 1, 1);

        public GraphController(
            Func<IUnitOfWork> unitOfWork,
            IBankAccountService bankAccountService,
            IAuthenticationService sessionService)
        {
            _unitOfWork = unitOfWork;
            _bankAccountService = bankAccountService;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Returns the history of the total balance for the given bank accounts
        /// </summary>
        /// <param name="years">Years to show</param>
        /// <param name="bankAccountIds">Number of bank accounts</param>
        /// <returns>Result in Json for display in a graph</returns>
        [HttpPost]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public async Task<JsonResult> BalanceHistory(int years, IList<int> bankAccountIds = null)
        {
            var userBankAccounts = await this._bankAccountService.GetActiveBankAccountsForCurrentUser();
            var currentUserId = await this._sessionService.GetCurrentUserId();

            IList<BankAccount> bankAccounts = bankAccountIds?.Any() == true ? userBankAccounts.Where(item => bankAccountIds.Any(y => y == item.Id)).ToList() : userBankAccounts;
            IList<int> bankAccountsIdFilter = bankAccounts.Select(x => x.Id).ToList();

            JsonResult result;
            if (bankAccounts.Count > 0)
            {
                List<decimal[]> sumPerDatesJson = new List<decimal[]>();

                int addYears;
                checked
                {
                    addYears = 0 - years;
                }

                DateTime startDate = DateTime.Now.AddYears(addYears);
                DateTime endDate = DateTime.Now;

                decimal accountBalance = 0;
                using (var unitOfWork = _unitOfWork())
                {
                    IList<Transaction> transactions = unitOfWork.TransactionRepository.FindAll(item => bankAccountsIdFilter.Any(y => y == item.BankAccountId) &&
                                                                                                 item.UserId == currentUserId &&
                                                                                                 item.Date >= startDate &&
                                                                                                 item.Date <= endDate).OrderBy(item => item.Date).ToList();

                    accountBalance = bankAccounts.Sum(item => item.StartBalance);

                    // Cast it to decimal? incase no transactions were found and sum returns null
                    accountBalance += unitOfWork.TransactionRepository.FindAll(item =>
                        bankAccountsIdFilter.Any(y => y == item.BankAccountId) && item.Date <= startDate).Sum(item => (decimal?)item.Amount).GetValueOrDefault();

                    List<IGrouping<DateTime, Transaction>> transactionsPerDate = transactions.GroupBy(item => item.Date).ToList();

                    // Add the beginning of the year transaction if there were previous transactions
                    if (accountBalance > bankAccounts.Sum(item => item.StartBalance) && transactionsPerDate.First().Key.Month != 1 &&
                        transactionsPerDate.First().Key.Day != 1)
                    {
                        sumPerDatesJson.Add(new[]
                        {
                        Convert.ToDecimal(startDate.Subtract(_utcStartDate).TotalMilliseconds),
                        accountBalance
                    });
                    }

                    foreach (IGrouping<DateTime, Transaction> groupedTransactions in transactionsPerDate)
                    {
                        accountBalance = groupedTransactions.Sum(item => item.Amount) + accountBalance;

                        sumPerDatesJson.Add(new[]
                        {
                        Convert.ToDecimal(groupedTransactions.Key.Subtract(_utcStartDate).TotalMilliseconds),
                        accountBalance
                    });
                    }
                    result = Json(new SinanceJsonResult
                    {
                        Success = true,
                        ObjectData = sumPerDatesJson
                    });
                }
            }
            else
                result = Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.Error
                });

            return result;
        }

        /// <summary>
        /// Retrieves the historic transactions graph for a bank account
        /// </summary>
        /// <param name="bankAccountIds">Ids of the bank accounts</param>
        /// <param name="year">Year to display transactions for</param>
        /// <returns>Data for display in a graph</returns>
        [HttpPost]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public async Task<JsonResult> BalanceHistoryPerYear(int year, IList<int> bankAccountIds = null)
        {
            var userBankAccounts = await this._bankAccountService.GetActiveBankAccountsForCurrentUser();
            var currentUserId = await this._sessionService.GetCurrentUserId();

            IList<BankAccount> bankAccounts = bankAccountIds != null && bankAccountIds.Any() ?
                userBankAccounts.Where(item => bankAccountIds.Any(y => y == item.Id)).ToList() : userBankAccounts;
            IList<int> validBankAccountIds = bankAccounts.Select(item => item.Id).ToList();

            JsonResult result;
            if (bankAccounts.Count > 0)
            {
                List<decimal[]> sumPerDatesJson = new List<decimal[]>();

                DateTime thisYearStart = new DateTime(year, 1, 1).Date;
                DateTime nextYearStart = new DateTime(year, 1, 1).AddYears(1).Date;

                using (var unitOfWork = _unitOfWork())
                {
                    IList<Transaction> transactions = unitOfWork.TransactionRepository.FindAll(item => validBankAccountIds.Any(y => y == item.BankAccountId) &&
                                                                                                 item.UserId == currentUserId &&
                                                                                                 item.Date >= thisYearStart &&
                                                                                                 item.Date < nextYearStart)
                    .OrderBy(item => item.Date).ToList();

                    var accountBalance = bankAccounts.Sum(item => item.StartBalance);

                    // Cast it to decimal? incase no transactions were found and sum returns null
                    accountBalance += unitOfWork.TransactionRepository.FindAll(item =>
                        validBankAccountIds.Any(y => y == item.BankAccountId) && item.Date < thisYearStart).Sum(item => (decimal?)item.Amount).GetValueOrDefault();

                    List<IGrouping<DateTime, Transaction>> transactionsPerDate = transactions.GroupBy(item => item.Date).ToList();

                    // Add the beginning of the year transaction if there were previous transactions
                    if (accountBalance > bankAccounts.Sum(item => item.StartBalance) && transactionsPerDate.First().Key.Month != 1 &&
                        transactionsPerDate.First().Key.Day != 1)
                    {
                        sumPerDatesJson.Add(new[]
                        {
                        Convert.ToDecimal(thisYearStart.Subtract(_utcStartDate).TotalMilliseconds),
                        accountBalance
                    });
                    }

                    foreach (IGrouping<DateTime, Transaction> groupedTransactions in transactionsPerDate)
                    {
                        accountBalance = groupedTransactions.Sum(item => item.Amount) + accountBalance;

                        sumPerDatesJson.Add(new[]
                        {
                        Convert.ToDecimal(groupedTransactions.Key.Subtract(_utcStartDate).TotalMilliseconds),
                        accountBalance
                    });
                    }
                    result = Json(new SinanceJsonResult
                    {
                        Success = true,
                        ObjectData = sumPerDatesJson
                    });
                }
            }
            else
                result = Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.Error
                });

            return result;
        }

        /// <summary>
        /// Creates the JSON data for the custom report monthly graph
        /// </summary>
        /// <param name="customReportId">Identifier of the custom report</param>
        /// <param name="year">What year to display</param>
        /// <returns>JSON encoded data for use in a Highcharts graph</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [HttpPost]
        public async Task<ContentResult> CustomReportMonthlyGraph(int customReportId, int? year)
        {
            var currentUserId = await this._sessionService.GetCurrentUserId();

            DateTime dateRangeStart = new DateTime(year ?? DateTime.Now.Year, 1, 1);
            DateTime dateRangeEnd = new DateTime(year ?? DateTime.Now.Year, 12, 31);

            using (var unitOfWork = _unitOfWork())
            {
                IList<int> reportCategories = unitOfWork.CustomReportCategoryRepository.FindAll(item => item.CustomReportId == customReportId).Select(item => item.CategoryId).ToList();

                IList<Transaction> transactions =
                    unitOfWork.TransactionRepository.FindAll(item => item.Date >= dateRangeStart && item.Date <= dateRangeEnd &&
                        item.UserId == currentUserId && item.Amount < 0 &&
                        item.TransactionCategories.Any(transactionCategory => reportCategories.Any(reportCategory => reportCategory == transactionCategory.CategoryId)),
                        "TransactionCategories", "TransactionCategories.Category");

                IDictionary<string, IDictionary<int, decimal>> reportDictionary = new Dictionary<string, IDictionary<int, decimal>>();

                foreach (Transaction transaction in transactions)
                {
                    if (transaction.TransactionCategories != null && transaction.TransactionCategories.Any())
                    {
                        foreach (TransactionCategory transactionCategory in transaction.TransactionCategories.Where(transactionCategory => reportCategories.Any(reportCategory => reportCategory == transactionCategory.CategoryId)))
                        {
                            Category category = transactionCategory.Category;

                            if (!reportDictionary.ContainsKey(category.Name))
                                reportDictionary.Add(category.Name, new Dictionary<int, decimal>
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
                            });

                            var amount = transactionCategory.Amount ?? transaction.Amount;

                            // Make sure the number is positive;
                            if (amount < 0)
                                amount *= -1;

                            reportDictionary[category.Name][transaction.Date.Month] += amount;
                        }
                    }
                    else
                    {
                        const string noCategoryName = "Geen categorie";
                        if (!reportDictionary.ContainsKey(noCategoryName))
                        {
                            reportDictionary.Add(noCategoryName, new Dictionary<int, decimal>
                        {
                            {1, 0},
                            {2, 0},
                            {3, 0},
                            {4, 0},
                            {5, 0},
                            {6, 0},
                            {7, 0},
                            {8, 0},
                            {9, 0},
                            {10, 0},
                            {11, 0},
                            {12, 0}
                        });
                        }

                        // Make sure the number is positive
                        reportDictionary[noCategoryName][transaction.Date.Month] += transaction.Amount < 0 ? transaction.Amount * -1 : transaction.Amount;
                    }
                }

                var expensesReportModel = new
                {
                    GraphCategories = SelectListHelper.CreateMonthSelectList().Select(item => item.Text).ToList(),
                    GraphData = new List<GraphData>()
                };

                foreach (var key in reportDictionary.Keys.OrderBy(item => item))
                {
                    expensesReportModel.GraphData.Add(new GraphData
                    {
                        Name = key,
                        Data = reportDictionary[key].Values.ToArray()
                    });
                }

                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var json = JsonConvert.SerializeObject(new SinanceJsonResult
                {
                    Success = true,
                    ObjectData = expensesReportModel
                }, Formatting.Indented, jsonSerializerSettings);

                return Content(json, "application/json");
            }
        }

        [HttpPost]
        public async Task<JsonResult> ExpensePercentagesPerMonth(int year, int month)
        {
            var currentUserId = await this._sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                // No need to sort this list, we loop through it by month numbers
                List<Transaction> transactions = unitOfWork.TransactionRepository
                .FindAll(
                    item =>
                        item.Date.Year == year &&
                        item.Date.Month == month &&
                        item.UserId == currentUserId &&
                        item.Amount < 0,
                    nameof(Transaction.TransactionCategories), "TransactionCategories.Category", "TransactionCategories.Category.ParentCategory")
                .ToList();

                var amountPerCategory = new Dictionary<Category, decimal>();
                var noneCategory = new Category()
                {
                    Name = "Geen"
                };

                foreach (var transaction in transactions)
                {
                    if (transaction.TransactionCategories.Any())
                    {
                        foreach (var transactionCategory in transaction.TransactionCategories.Where(item => item.CategoryId != 69))
                        {
                            var categoryToUse = transactionCategory.Category.ParentCategory ?? transactionCategory.Category;

                            if (!amountPerCategory.ContainsKey(categoryToUse))
                            {
                                amountPerCategory.Add(categoryToUse, 0M);
                            }

                            amountPerCategory[categoryToUse] += (transactionCategory.Amount ?? transaction.Amount) * -1;
                        }
                    }
                    else
                    {
                        if (!amountPerCategory.ContainsKey(noneCategory))
                        {
                            amountPerCategory.Add(noneCategory, 0M);
                        }
                        amountPerCategory[noneCategory] += transaction.Amount * -1;
                    }
                }

                var total = amountPerCategory.Sum(x => x.Value);

                var jsonData = amountPerCategory.Select(x => new GraphDataEntry
                {
                    Name = x.Key.Name,
                    Y = (x.Value / total) * 100
                });

                return Json(new SinanceJsonResult
                {
                    Success = true,
                    ObjectData = jsonData
                });
            }
        }

        /// <summary>
        /// Calculates the profit per month for the given year
        /// </summary>
        /// <param name="year">Number of the year to calculate for</param>
        /// <returns>Data for display in a graph</returns>
        [HttpPost]
        public async Task<JsonResult> ProfitPerMonthForYear(int year)
        {
            var currentUserId = await this._sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                // No need to sort this list, we loop through it by month numbers
                List<IGrouping<int, Transaction>> transactionsPerMonth = unitOfWork.TransactionRepository
                .FindAll(item => item.Date.Year == year && item.UserId == currentUserId && item.BankAccount.IncludeInProfitLossGraph == true)
                .GroupBy(item => item.Date.Month)
                .ToList();

                IList<decimal> jsonData = new List<decimal>();

                for (var month = 1; month <= 12; month++)
                {
                    IGrouping<int, Transaction> transactions = transactionsPerMonth.SingleOrDefault(item => item.Key == month);
                    jsonData.Add(transactions?.Sum(item => item.Amount) ?? 0);
                }

                return Json(new SinanceJsonResult
                {
                    Success = true,
                    ObjectData = jsonData
                });
            }
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Calculations;
using Sinance.Business.Services;
using Sinance.Communication;
using Sinance.Communication.Model.BankAccount;
using Sinance.Web;
using Sinance.Web.Helper;
using Sinance.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Controllers
{
    /// <summary>
    /// Controller for all graph actions
    /// </summary>
    [Authorize]
    public class GraphController : Controller
    {
        private readonly IBalanceHistoryCalculation _balanceHistoryCalculation;
        private readonly ICustomReportService _customReportService;
        private readonly IExpenseCalculation _expenseCalculation;
        private readonly IExpensePercentageCalculation _expensePercentageCalculation;
        private readonly IProfitLossCalculation _profitLossCalculation;

        public GraphController(
            ICustomReportService customReportService,
            IExpenseCalculation expenseCalculation,
            IBalanceHistoryCalculation balanceHistoryCalculation,
            IExpensePercentageCalculation expensePercentageCalculation,
            IProfitLossCalculation profitLossCalculation)
        {
            _customReportService = customReportService;
            _expenseCalculation = expenseCalculation;
            _balanceHistoryCalculation = balanceHistoryCalculation;
            _expensePercentageCalculation = expensePercentageCalculation;
            _profitLossCalculation = profitLossCalculation;
        }

        /// <summary>
        /// Returns the history of the total balance for the given bank accounts
        /// </summary>
        /// <param name="years">Years to show</param>
        /// <param name="bankAccountIds">Number of bank accounts</param>
        /// <returns>Result in Json for display in a graph</returns>
        [HttpPost]
        public async Task<IActionResult> BalanceHistory(int months, IList<int> bankAccountIds = null)
        {
            try
            {
                var balancehistory = await _balanceHistoryCalculation.BalanceHistoryFromMonthsInPast(months, bankAccountIds);

                return Json(new SinanceJsonResult
                {
                    Success = true,
                    ObjectData = balancehistory
                });
            }
            catch (Exception)
            {
                return Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.Error
                });
            }
        }

        /// <summary>
        /// Retrieves the historic transactions graph for a bank account
        /// </summary>
        /// <param name="bankAccountIds">Ids of the bank accounts</param>
        /// <param name="year">Year to display transactions for</param>
        /// <returns>Data for display in a graph</returns>
        [HttpPost]
        public async Task<IActionResult> BalanceHistoryPerYear(int year, IList<int> bankAccountIds = null)
        {
            try
            {
                var balancehistory = await _balanceHistoryCalculation.BalanceHistoryForYear(year, bankAccountIds);

                return Json(new SinanceJsonResult
                {
                    Success = true,
                    ObjectData = balancehistory
                });
            }
            catch (Exception)
            {
                return Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.Error
                });
            }
        }

        /// <summary>
        /// Creates the JSON data for the custom report monthly graph
        /// </summary>
        /// <param name="customReportId">Identifier of the custom report</param>
        /// <param name="year">What year to display</param>
        /// <returns>JSON encoded data for use in a Highcharts graph</returns>
        [HttpPost]
        public async Task<IActionResult> CustomReportMonthlyGraph(int customReportId, int year)
        {
            var report = await _customReportService.GetCustomReportByIdForCurrentUser(customReportId);
            var reportCategories = report.Categories.Select(x => x.CategoryId);

            var expensesPerMonth = await _expenseCalculation.ExpensePerCategoryIdPerMonthForYear(year, reportCategories);

            var expensesReportModel = new
            {
                GraphCategories = SelectListHelper.CreateMonthSelectList().Select(item => item.Text).ToList(),
                GraphData = new List<GraphData>()
            };

            foreach (var key in expensesPerMonth.Keys.OrderBy(item => item))
            {
                expensesReportModel.GraphData.Add(new GraphData
                {
                    Name = key,
                    Data = expensesPerMonth[key].Values.ToArray()
                });
            }

            return Json(new SinanceJsonResult
            {
                Success = true,
                ObjectData = expensesReportModel
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExpensePercentagesPerCategory(DateTime startDate, DateTime endDate)
        {
            try
            {
                var expensePercentagePerCategoryName = await _expensePercentageCalculation.ExpensePercentagePerCategoryNameForMonth(startDate, endDate);

                return Json(new SinanceJsonResult
                {
                    Success = true,
                    ObjectData = expensePercentagePerCategoryName.Select(x => new GraphDataEntry
                    {
                        Name = x.Key,
                        Y = x.Value * 100
                    })
                });
            }
            catch (Exception)
            {
                return Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.Error
                });
            }
        }


        /// <summary>
        /// Calculates the profit per month for the given year
        /// </summary>
        /// <param name="year">Number of the year to calculate for</param>
        /// <returns>Data for display in a graph</returns>
        [HttpPost]
        public async Task<IActionResult> ProfitPerMonthForYearGrouped(int year)
        {
            var profitPerMonth = await _profitLossCalculation.CalculateProfitLosstPerMonthForYearGrouped(year);

            var series = profitPerMonth.Select(x => new 
            {
                name = Enum.GetName(typeof(BankAccountType), x.AccountType),
                data = x.ProfitPerMonth.AsEnumerable(),
                type = "column"
            }).ToList();

            series.Add(new
            {
                name = "Total",
                data = profitPerMonth.SelectMany(x => x.ProfitPerMonth)
                                        .Select((v, i) => new { Value = v, Index = i % profitPerMonth.First().ProfitPerMonth.Count})
                                        .GroupBy(x => x.Index)
                                        .Select(y => y.Sum(z => z.Value)),
                type = "line"
            });

            return Json(new SinanceJsonResult
            {
                Success = true,
                ObjectData = series
            });
        }
        

        /// <summary>
        /// Calculates the profit per month for the given year
        /// </summary>
        /// <param name="year">Number of the year to calculate for</param>
        /// <returns>Data for display in a graph</returns>
        [HttpPost]
        public async Task<IActionResult> ProfitPerMonthForYear(int year)
        {
            var profitPerMonth = await _profitLossCalculation.CalculateProfitLosstPerMonthForYear(year);

            return Json(new SinanceJsonResult
            {
                Success = true,
                ObjectData = profitPerMonth
            });
        }
    }
}
using Sinance.Business.Classes;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Business.Calculations;
using Sinance.Business.Services;

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
        public async Task<IActionResult> BalanceHistory(int years, IList<int> bankAccountIds = null)
        {
            try
            {
                var balancehistory = await _balanceHistoryCalculation.BalanceHistoryFromYearInPast(years, bankAccountIds);

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
        public async Task<IActionResult> ExpensePercentagesPerMonth(int year, int month)
        {
            try
            {
                var expensePercentagePerCategoryName = await _expensePercentageCalculation.ExpensePercentagePerCategoryNameForMonth(year, month);

                return Json(new SinanceJsonResult
                {
                    Success = true,
                    ObjectData = expensePercentagePerCategoryName
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
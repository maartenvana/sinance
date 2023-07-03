using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Calculations;
using Sinance.Business.Exceptions;
using Sinance.Business.Services;
using Sinance.Business.Services.Categories;
using Sinance.Communication.Model.CustomReport;
using Sinance.Web;
using Sinance.Web.Helper;
using Sinance.Web.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Controllers
{
    /// <summary>
    /// Controller for all reports
    /// </summary>
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ICustomReportService _customReportService;
        private readonly IExpenseCalculation _expenseCalculation;
        private readonly IIncomeCalculation _incomeCalculation;
        private readonly IYearlyOverviewCalculation _yearlyOverviewCalculation;

        public ReportController(
            ICategoryService categoryService,
            ICustomReportService customReportService,
            IIncomeCalculation incomeCalculation,
            IExpenseCalculation expenseCalculation,
            IYearlyOverviewCalculation yearlyOverviewCalculation)
        {
            _categoryService = categoryService;
            _customReportService = customReportService;
            _incomeCalculation = incomeCalculation;
            _expenseCalculation = expenseCalculation;
            _yearlyOverviewCalculation = yearlyOverviewCalculation;
        }

        /// <summary>
        /// Action for displaying the add page for a custom graph report
        /// </summary>
        /// <returns>Add page for a custom graph report</returns>
        public async Task<IActionResult> AddCustomReport()
        {
            var allCategories = await _categoryService.GetAllCategoriesForCurrentUser();

            var availableCategories = allCategories.Select(item => new BasicCheckBoxItem
            {
                Id = item.Id,
                Name = item.Name,
            }).ToList();

            return View("UpsertCustomReport", new UpsertCustomReportModel
            {
                AvailableCategories = availableCategories,
                CustomReport = new CustomReportModel()
            });
        }

        /// <summary>
        /// Displays the Custom report page
        /// </summary>
        /// <param name="reportId">Identifier of the custom report</param>
        /// <returns>A custom report page</returns>
        public async Task<IActionResult> CustomReport(int reportId)
        {
            var report = await _customReportService.GetCustomReportByIdForCurrentUser(reportId);

            return View("CustomReport", report);
        }

        /// <summary>
        /// Edit action for custom reports
        /// </summary>
        /// <param name="reportId">Identifier of the report to edit</param>
        /// <returns>View with the custom report to edit</returns>
        public async Task<IActionResult> EditCustomReport(int reportId)
        {
            var allCategories = await _categoryService.GetAllCategoriesForCurrentUser();

            try
            {
                var report = await _customReportService.GetCustomReportByIdForCurrentUser(reportId);

                var availableCategories = allCategories.Select(category => new BasicCheckBoxItem
                {
                    Id = category.Id,
                    Name = category.Name,
                    Checked = report.Categories.Any(reportCategory => reportCategory.CategoryId == category.Id)
                }).ToList();

                return View("UpsertCustomReport", new UpsertCustomReportModel
                {
                    AvailableCategories = availableCategories,
                    CustomReport = report
                });
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CustomReportNotFound);
                return View("Home", "Index");
            }
        }

        /// <summary>
        /// Action for displaying all the expenses per category
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ExpenseOverview()
        {
            var startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

            var model = await _expenseCalculation.BiMonthlyExpensePerCategoryReport(startMonth);

            return View("ExpenseOverview", model);
        }

        /// <summary>
        /// Action for displaying all the income per category
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IncomeOverview()
        {
            var startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

            var model = await _incomeCalculation.BiMonthlyIncomePerCategoryReport(startMonth);

            return View("IncomeOverview", model);
        }

        /// <summary>
        /// Upserts a custom report to the database
        /// </summary>
        /// <param name="model">Model to upsert</param>
        /// <returns>Redirect to the upserted custom report</returns>
        public async Task<IActionResult> UpsertCustomReport(UpsertCustomReportModel model)
        {
            if (!ModelState.IsValid)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.Error);
            }

            if (model.CustomReport.Id > 0)
            {
                try
                {
                    model.CustomReport.Categories = model.AvailableCategories.Where(x => x.Checked).Select(x => new CustomReportCategoryModel
                    {
                        CategoryId = x.Id
                    }).ToList();

                    await _customReportService.UpdateCustomReport(model.CustomReport);
                }
                catch (NotFoundException)
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CustomReportNotFound);
                    return View("Home", "Index");
                }
            }
            else
            {
                model.CustomReport.Categories = model.AvailableCategories.Where(x => x.Checked).Select(x => new CustomReportCategoryModel
                {
                    CategoryId = x.Id
                }).ToList();

                await _customReportService.CreateCustomReport(model.CustomReport);
            }

            return RedirectToAction("CustomReport", new { reportId = model.CustomReport.Id });
        }

        public async Task<IActionResult> YearOverview(int? year)
        {
            var report = await _yearlyOverviewCalculation.CalculateForYear(year.GetValueOrDefault(DateTime.Now.Year));

            return View("YearReport", report);
        }
    }
}
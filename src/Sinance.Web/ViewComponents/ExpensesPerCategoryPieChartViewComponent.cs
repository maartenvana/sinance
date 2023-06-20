using Microsoft.AspNetCore.Mvc;
using System;

namespace Sinance.Web.ViewComponents;

public class ExpensesPerCategoryPieChartModel
{
    public DateTime EndDate { get; set; }
    public DateTime StartDate { get; set; }
}

public class ExpensesPerCategoryPieChartViewComponent : ViewComponent
{
    public ExpensesPerCategoryPieChartViewComponent()
    {
    }

    public IViewComponentResult Invoke(DateTime startDate, DateTime endDate)
    {
        return View(new ExpensesPerCategoryPieChartModel
        {
            StartDate = startDate,
            EndDate = endDate
        });
    }
}
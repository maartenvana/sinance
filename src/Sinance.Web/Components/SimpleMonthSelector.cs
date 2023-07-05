using Microsoft.AspNetCore.Mvc;
using Sinance.Web.Model;
using System;

namespace Sinance.Web.Components;

public class SimpleMonthSelector : ViewComponent
{
    public IViewComponentResult Invoke(DateTime currentDate, string action, string controller)
    {
        var monthYearModel = new MonthYearSelectionModel
        {
            CurrentDate = currentDate,
            Action = action,
            Controller = controller
        };

        return View(monthYearModel);
    }
}
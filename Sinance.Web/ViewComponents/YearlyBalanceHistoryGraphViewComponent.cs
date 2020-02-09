using Microsoft.AspNetCore.Mvc;

namespace Sinance.Web.ViewComponents
{
    public class YearlyBalanceHistoryGraphViewComponent : ViewComponent
    {
        public YearlyBalanceHistoryGraphViewComponent()
        {
        }

        public IViewComponentResult Invoke(int year)
        {
            return View(year);
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace Sinance.Web.ViewComponents
{
    public class BalanceHistoryGraphViewComponent : ViewComponent
    {
        public BalanceHistoryGraphViewComponent()
        {
        }

        public IViewComponentResult Invoke(int months)
        {
            return View(months);
        }
    }
}
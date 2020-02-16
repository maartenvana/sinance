using Microsoft.AspNetCore.Mvc;

namespace Sinance.Web.ViewComponents
{
    public class YearlyProfitGraphViewComponent : ViewComponent
    {
        public YearlyProfitGraphViewComponent()
        {
        }

        public IViewComponentResult Invoke(int year, bool canChangeYear)
        {
            return View(new YearlyProfitGraphViewComponentModel
            {
                InitialYear = year,
                CanChangeYear = canChangeYear
            });
        }
    }

    public class YearlyProfitGraphViewComponentModel
    {
        public bool CanChangeYear { get; set; }
        public int InitialYear { get; set; }
    }
}
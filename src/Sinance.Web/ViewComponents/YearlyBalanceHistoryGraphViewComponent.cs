using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Calculations;
using Sinance.Communication.Model.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Web.ViewComponents;

public class YearlyBalanceHistoryGraphViewComponent : ViewComponent
{
    private readonly IBalanceHistoryCalculation balanceHistoryCalculation;

    public YearlyBalanceHistoryGraphViewComponent(IBalanceHistoryCalculation balanceHistoryCalculation)
    {
        this.balanceHistoryCalculation = balanceHistoryCalculation;
    }

    public async Task<IViewComponentResult> InvokeAsync(int year, bool grouped, int[] filter)
    {
        List<BalanceHistoryRecord> balanceHistoryRecords;

        if (grouped)
        {
            balanceHistoryRecords = await balanceHistoryCalculation.BalanceHistoryForYearGroupedByType(year, filter);
        }
        else
        {
            balanceHistoryRecords = await balanceHistoryCalculation.BalanceHistoryForYear(year, filter);
        }

        return View(balanceHistoryRecords);
    }
}
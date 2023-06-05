using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Calculations;
using Sinance.Communication.Model.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Web.ViewComponents;

public class BalanceHistoryGraphViewComponent : ViewComponent
{
    private readonly IBalanceHistoryCalculation balanceHistoryCalculation;

    public BalanceHistoryGraphViewComponent(IBalanceHistoryCalculation balanceHistoryCalculation)
    {
        this.balanceHistoryCalculation = balanceHistoryCalculation;
    }

    public async Task<IViewComponentResult> InvokeAsync(int months, bool grouped, int[] filter)
    {
        List<BalanceHistoryRecord> balanceHistoryRecords;

        if (grouped)
        {
            balanceHistoryRecords = await balanceHistoryCalculation.BalanceHistoryFromMonthsInPastGroupedByType(months, filter);
        }
        else
        {
            balanceHistoryRecords = await balanceHistoryCalculation.BalanceHistoryFromMonthsInPast(months, filter);
        }

        return View(balanceHistoryRecords);
    }
}
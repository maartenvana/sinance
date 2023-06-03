using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Services;
using Sinance.Business.Services.BankAccounts;
using Sinance.Web.Model;
using System.Threading.Tasks;

namespace Sinance.Web.Components
{
    public class SideNavigation : ViewComponent
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly ICustomReportService _customReportService;

        public SideNavigation(
            IBankAccountService bankAccountService,
            ICustomReportService customReportService)
        {
            _bankAccountService = bankAccountService;
            _customReportService = customReportService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();
            var customReports = await _customReportService.GetCustomReportsForCurrentUser();

            var model = new NavigationViewModel
            {
                BankAccounts = bankAccounts,
                CustomReports = customReports
            };

            return View(model);
        }
    }
}
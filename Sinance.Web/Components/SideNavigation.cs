using Sinance.Business.Services;
using Sinance.Web.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Sinance.Business.Services.BankAccounts;

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
using Sinance.Business.Services;
using Sinance.Web.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Sinance.Web.Components
{
    public class SideNavigation : ViewComponent
    {
        private readonly IBankAccountService bankAccountService;
        private readonly ICustomReportService customReportService;

        public SideNavigation(
            IBankAccountService bankAccountService,
            ICustomReportService customReportService)
        {
            this.bankAccountService = bankAccountService;
            this.customReportService = customReportService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var bankAccounts = await this.bankAccountService.GetActiveBankAccountsForCurrentUser();
            var customReports = await this.customReportService.GetCustomReportsForCurrentUser();

            var model = new NavigationModel
            {
                BankAccounts = bankAccounts,
                CustomReports = customReports
            };

            return View(model);
        }
    }
}
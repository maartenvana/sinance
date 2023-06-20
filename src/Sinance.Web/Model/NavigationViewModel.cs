using Sinance.Communication.Model.BankAccount;
using Sinance.Communication.Model.CustomReport;
using System.Collections.Generic;

namespace Sinance.Web.Model;

public class NavigationViewModel
{
    public IList<BankAccountModel> BankAccounts { get; set; }

    public IList<CustomReportModel> CustomReports { get; set; }
}
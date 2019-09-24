using Sinance.Domain.Entities;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class NavigationModel
    {
        public IList<BankAccount> BankAccounts { get; set; }

        public IList<CustomReport> CustomReports { get; set; }
    }
}
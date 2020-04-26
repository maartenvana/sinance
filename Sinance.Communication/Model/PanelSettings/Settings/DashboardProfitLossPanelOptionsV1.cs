using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Communication.Model.PanelSettings
{
    public class DashboardProfitLossPanelOptionsV1
    {
        public Dictionary<int, int> BankAccountGroups { get; set; }

        public Dictionary<int, string> GroupNames { get; set; }

        public bool DisplayTotal { get; set; }
    }
}

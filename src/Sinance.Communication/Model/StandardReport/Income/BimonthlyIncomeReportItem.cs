using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.StandardReport.Income
{
    /// <summary>
    /// Bimonthly exepense report
    /// </summary>
    public class BimonthlyIncomeReportItem
    {
        /// <summary>
        /// Date of the previous month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MMMM}")]
        public DateTime FirstMonth { get; set; }

        /// <summary>
        /// Incomes to show (parent and child's
        /// </summary>
        public IList<BimonthlyIncome> Incomes { get; set; }

        /// <summary>
        /// Last month total expenses
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PreviousMonthTotal { get; set; }

        /// <summary>
        /// Date of this month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MMMM}")]
        public DateTime SecondMonth { get; set; }

        /// <summary>
        /// This month total expenses
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ThisMonthTotal { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.StandardReport.Expense
{
    /// <summary>
    /// Bimonthly exepense report
    /// </summary>
    public class BimonthlyExpenseReportItem
    {
        /// <summary>
        /// Incomes to show (parent and child's
        /// </summary>
        public IList<BimonthlyExpense> Expenses { get; set; }

        /// <summary>
        /// Date of the previous month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MMMM}")]
        public DateTime FirstMonthDate { get; set; }

        /// <summary>
        /// Last month total expenses
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PreviousMonthTotal { get; set; }

        /// <summary>
        /// Remaining amount for this month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Date of this month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MMMM}")]
        public DateTime SecondMonthDate { get; set; }

        /// <summary>
        /// This month total expenses
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ThisMonthTotal { get; set; }
    }
}
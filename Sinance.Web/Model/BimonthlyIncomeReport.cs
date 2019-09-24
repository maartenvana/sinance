using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Bimonthly exepense report
    /// </summary>
    public class BimonthlyIncomeReport
    {
        /// <summary>
        /// Incomes to show (parent and child's
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Its a model!")]
        public IList<BimonthlyIncome> Incomes { get; set; }

        /// <summary>
        /// Date of the previous month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MMMM}")]
        public DateTime PreviousMonthDate { get; set; }

        /// <summary>
        /// Last month total expenses
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PreviousMonthTotal { get; set; }

        /// <summary>
        /// Date of this month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MMMM}")]
        public DateTime ThisMonthDate { get; set; }

        /// <summary>
        /// This month total expenses
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ThisMonthTotal { get; set; }
    }
}
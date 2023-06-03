using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.StandardReport.Income
{
    /// <summary>
    /// Regular expense
    /// </summary>
    public class BimonthlyIncome
    {
        /// <summary>
        /// Amount of the current month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AmountNow { get; set; }

        /// <summary>
        /// Amount of the previous month
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AmountPrevious { get; set; }

        /// <summary>
        /// Child bimonthly expenses
        /// </summary>
        public IList<BimonthlyIncome> ChildBimonthlyIncomes { get; private set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BimonthlyIncome()
        {
            ChildBimonthlyIncomes = new List<BimonthlyIncome>();
        }
    }
}
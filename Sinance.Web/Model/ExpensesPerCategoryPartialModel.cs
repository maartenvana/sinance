using Sinance.Domain.Entities;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model containg the data for display of the expenses per category report
    /// </summary>
    public class ExpensesPerCategoryPartialModel
    {
        /// <summary>
        /// Name of the current category
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Values for display in the graph
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Model")]
        public IList<decimal[]> GraphValues { get; set; }

        /// <summary>
        /// Transactions to display
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Model")]
        public IList<Transaction> Transactions { get; set; }
    }
}
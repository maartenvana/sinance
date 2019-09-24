using Sinance.Domain.Entities;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Account overview model
    /// </summary>
    public class AccountOverviewModel
    {
        /// <summary>
        /// Bank account to display
        /// </summary>
        public BankAccount Account { get; set; }

        /// <summary>
        /// Total current balance of the account
        /// </summary>
        public decimal AccountBalance { get; set; }

        /// <summary>
        /// Available categories to choose from for quick editing a category of a transaction
        /// </summary>
        public IEnumerable<Category> AvailableCategories { get; set; }

        /// <summary>
        /// Transaction for this bank account
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<Transaction> Transactions { get; set; }
    }
}
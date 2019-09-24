using Sinance.Domain.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Result model for the save of a import
    /// </summary>
    public class SaveImportResultModel
    {
        /// <summary>
        /// Transactions that failed to convert/save
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Its a model")]
        public IList<string[]> FailedTransactions { get; set; }

        /// <summary>
        /// Transactions that were saved
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Its a model")]
        public IEnumerable<Transaction> SavedTransactions { get; set; }
    }
}
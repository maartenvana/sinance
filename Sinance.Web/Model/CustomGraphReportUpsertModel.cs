using System.Collections.Generic;
using System.ComponentModel;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model for upserting a custom graph report
    /// </summary>
    public class CustomGraphReportUpsertModel
    {
        /// <summary>
        /// List of all available categories for the report to use
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<BasicCheckBoxItem> AvailableCategories { get; set; }

        /// <summary>
        /// Id of the report to update
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the report
        /// </summary>
        [DisplayName("Naam")]
        public string Name { get; set; }
    }
}
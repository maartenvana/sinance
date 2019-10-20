using Sinance.Communication.Model.CustomReport;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model for upserting a custom graph report
    /// </summary>
    public class UpsertCustomReportModel
    {
        /// <summary>
        /// List of all available categories for the report to use
        /// </summary>
        public IList<BasicCheckBoxItem> AvailableCategories { get; set; }

        public CustomReportModel CustomReport { get; set; }
    }
}
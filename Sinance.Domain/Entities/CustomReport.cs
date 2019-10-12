﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Entity class for custom reports
    /// </summary>
    public class CustomReport : UserEntityBase
    {
        /// <summary>
        /// Name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Collection of categories to use in the report
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<CustomReportCategory> ReportCategories { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomReport()
        {
            ReportCategories = new HashSet<CustomReportCategory>();
        }
    }
}
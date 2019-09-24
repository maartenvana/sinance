using Sinance.Business.Classes;
using Sinance.Domain.Entities;
using Sinance.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model for importing a csv to the database
    /// </summary>
    public class ImportModel
    {
        /// <summary>
        /// Available bank accounts to save transaction to or map unknown bank accounts
        /// </summary>
        public IList<SelectListItem> AvailableAccounts { get; set; }

        /// <summary>
        /// Available import banks to choose from
        /// </summary>
        public IList<ImportBank> AvailableImportBanks { get; set; }

        /// <summary>
        /// Bank account to import to
        /// </summary>
        [Display(ResourceType = typeof(Resources), Name = "ImportToBankAccount")]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ChooseBankAccount")]
        public int BankAccountId { get; set; }

        /// <summary>
        /// The current bank account type to import
        /// </summary>
        public int ImportBankId { get; set; }

        /// <summary>
        /// Rows to import
        /// </summary>
        public IList<ImportRow> ImportRows { get; set; }
    }
}
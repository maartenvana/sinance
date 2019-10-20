using Sinance.Communication.Model.BankAccount;
using System.Collections.Generic;

namespace Sinance.Communication.Model.Import
{
    /// <summary>
    /// Model for importing a csv to the database
    /// </summary>
    public class ImportModel
    {
        /// <summary>
        /// Available bank accounts to save transaction to or map unknown bank accounts
        /// </summary>
        public IList<BankAccountModel> AvailableAccounts { get; set; }

        /// <summary>
        /// Available import banks to choose from
        /// </summary>
        public IList<ImportBankModel> AvailableImportBanks { get; set; }

        /// <summary>
        /// Bank account to import to
        /// </summary>
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
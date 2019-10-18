using Sinance.Communication.BankAccount;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Storage.Entities
{
    /// <summary>
    /// Bank account entity
    /// </summary>
    public class BankAccountEntity : UserEntityBase
    {
        /// <summary>
        /// Bank account type
        /// </summary>
        public BankAccountType AccountType { get; set; }

        /// <summary>
        /// Current balance
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:C2}")]
        public decimal? CurrentBalance { get; set; }

        /// <summary>
        /// If the account is disabled and should not be shown anymore
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// If the account should be included in profit/loss graphs
        /// </summary>
        public bool IncludeInProfitLossGraph { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Start balance
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = false, DataFormatString = "{0:C2}")]
        public decimal StartBalance { get; set; }

        /// <summary>
        /// Transactions
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TransactionEntity> Transactions { get; set; } = new HashSet<TransactionEntity>();
    }
}
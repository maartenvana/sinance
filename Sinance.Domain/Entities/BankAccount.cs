using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Bank account entity
    /// </summary>
    public class BankAccount : EntityBase
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
        public virtual ICollection<Transaction> Transactions { get; set; }

        /// <summary>
        /// User associated with this bank account
        /// </summary>
        [ForeignKey("UserId")]
        public virtual SinanceUser User { get; set; }

        /// <summary>
        /// User id associated with this bank account
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "EF needs virtual collections for lazy loading")]
        public BankAccount()
        {
            Transactions = new HashSet<Transaction>();
        }

        /// <summary>
        /// Updates the current instance with the values from the given entity
        /// </summary>
        /// <param name="name">Name of the bank account</param>
        /// <param name="startBalance">Start balance of the bank account</param>
        /// <param name="accountType">Type of the bank account</param>
        /// <param name="disabled">If the account is disabled</param>
        public void Update(string name, decimal startBalance, bool disabled, BankAccountType accountType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StartBalance = startBalance;
            AccountType = accountType;
            Disabled = disabled;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Transaction entity
    /// </summary>
    public class Transaction : EntityBase
    {
        /// <summary>
        /// Account number from
        /// </summary>
        [Display(Name = "Rekeningnummer")]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        [Display(Name = "Bedrag")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Amount is negative
        /// </summary>
        public bool AmountIsNegative { get; set; }

        /// <summary>
        /// Bank account
        /// </summary>
        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; }

        /// <summary>
        /// Bank account id
        /// </summary>
        public int BankAccountId { get; set; }

        /// <summary>
        /// Date
        /// </summary>
        [Required]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Display(Name = "Omschrijving")]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Destination account
        /// </summary>
        [Display(Name = "Tegenrekening")]
        [StringLength(50)]
        public string DestinationAccount { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Required]
        [Display(Name = "Titel")]
        [StringLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// Transaction categories
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TransactionCategory> TransactionCategories { get; set; }

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
        /// Default constructors
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "EF needs virtual collections for lazy loading")]
        public Transaction()
        {
            TransactionCategories = new HashSet<TransactionCategory>();
        }

        /// <summary>
        /// Updates the current instance of the transaction
        /// </summary>
        /// <param name="name">Name to use</param>
        /// <param name="description">Description to use</param>
        /// <param name="destinationAccount">Destination account to use</param>
        /// <param name="amount">Amount to use</param>
        /// <param name="date">Date to use</param>
        /// <param name="bankAccountId">Bank account id to use</param>
        public void Update(string name, string description, string destinationAccount, decimal amount, DateTime date, int bankAccountId)
        {
            Name = name;
            Description = description;
            DestinationAccount = destinationAccount;
            Amount = amount;
            Date = date;
            BankAccountId = bankAccountId;
        }
    }
}
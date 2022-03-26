using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Storage.Entities
{
    /// <summary>
    /// Transaction entity
    /// </summary>
    public class SourceTransactionEntity : UserEntityBase
    {
        /// <summary>
        /// Account number from
        /// </summary>
        [StringLength(50)]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Bank account
        /// </summary>
        [ForeignKey("BankAccountId")]
        public BankAccountEntity BankAccount { get; set; }

        /// <summary>
        /// Bank account id
        /// </summary>
        public int BankAccountId { get; set; }

        /// <summary>
        /// Date
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Destination account
        /// </summary>
        [StringLength(50)]
        public string DestinationAccount { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [StringLength(255)]
        public string Name { get; set; }

        public ICollection<TransactionEntity> Transactions { get; set; } = new HashSet<TransactionEntity>();

        public CategoryEntity Category { get; set; }

        public int? CategoryId { get; set; }
    }
}
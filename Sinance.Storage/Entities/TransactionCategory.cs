using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Storage.Entities
{
    /// <summary>
    /// Transaction category entity
    /// </summary>
    public class TransactionCategory : EntityBase
    {
        /// <summary>
        /// Amount
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        [ForeignKey("CategoryId")]
        public virtual CategoryEntity Category { get; set; }

        /// <summary>
        /// Category id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Transaction
        /// </summary>
        [ForeignKey("TransactionId")]
        public virtual TransactionEntity Transaction { get; set; }

        /// <summary>
        /// Transaction id
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// Updates the current instance with the values from the given entity
        /// </summary>
        /// <param name="updatedTransactionCategory">Entity to use properties from for updating</param>
        public void Update(TransactionCategory updatedTransactionCategory)
        {
            if (updatedTransactionCategory == null)
                throw new ArgumentNullException(nameof(updatedTransactionCategory));

            Amount = updatedTransactionCategory.Amount;
            CategoryId = updatedTransactionCategory.CategoryId;
            TransactionId = updatedTransactionCategory.TransactionId;
        }
    }
}
using Sinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Detail model for displaying the details of a transaction
    /// </summary>
    public class TransactionModel
    {
        /// <summary>
        /// Amount
        /// </summary>
        [Display(Name = "Bedrag")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Available categories to choose from
        /// </summary>
        [Display(Name = "Categorieën")]
        public IEnumerable<SelectListItem> AvailableCategories { get; set; }

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
        /// Id of the transaction
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Display(Name = "Titel")]
        [StringLength(255)]
        [Required(ErrorMessage = "{0} is vereist")]
        public string Name { get; set; }

        /// <summary>
        /// Categories applied to the transaction
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "We need an ilist for the view, model so ok to be not readonly")]
        public IList<TransactionCategory> TransactionCategories { get; set; }

        /// <summary>
        /// Creates a transaction model from the given transaction
        /// </summary>
        /// <param name="transaction">Transaction to use</param>
        /// <returns>The created transaction model</returns>
        public static TransactionModel CreateTransactionModel(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            return new TransactionModel
            {
                Id = transaction.Id,
                Name = transaction.Name,
                BankAccountId = transaction.BankAccountId,
                Amount = transaction.Amount,
                Date = transaction.Date,
                TransactionCategories = transaction.TransactionCategories != null ? transaction.TransactionCategories.ToList() : new List<TransactionCategory>(),
                Description = transaction.Description,
                DestinationAccount = transaction.DestinationAccount
            };
        }
    }
}
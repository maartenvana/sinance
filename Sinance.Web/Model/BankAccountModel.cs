using Sinance.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model for bank account
    /// </summary>
    public class BankAccountModel
    {
        /// <summary>
        /// Type of the bank account
        /// </summary>
        [Display(Name = "Rekening type")]
        public BankAccountType AccountType { get; set; }

        /// <summary>
        /// Current balance
        /// </summary>
        [Display(Name = "Huidige balans")]
        public decimal? CurrentBalance { get; set; }

        /// <summary>
        /// Type of the bank account
        /// </summary>
        [Display(Name = "Inactief")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Id for the current bank account
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Required]
        [Display(Name = "Naam")]
        public string Name { get; set; }

        /// <summary>
        /// Start balance
        /// </summary>
        [Display(Name = "Start balans")]
        public decimal StartBalance { get; set; }

        /// <summary>
        /// Creates a bank account model from the given entity
        /// </summary>
        /// <param name="bankAccount">Bank account entity to use</param>
        /// <returns>Created bank account</returns>
        public static BankAccountModel CreateBankAccountModel(BankAccount bankAccount)
        {
            if (bankAccount == null)
                throw new ArgumentNullException(nameof(bankAccount));

            return new BankAccountModel
            {
                Id = bankAccount.Id,
                Name = bankAccount.Name,
                StartBalance = bankAccount.StartBalance,
                CurrentBalance = bankAccount.CurrentBalance,
                AccountType = bankAccount.AccountType,
                Disabled = bankAccount.Disabled
            };
        }
    }
}
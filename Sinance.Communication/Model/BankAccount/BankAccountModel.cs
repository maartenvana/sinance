using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.BankAccount
{
    public class BankAccountModel
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
        /// Id of the account
        /// </summary>
        public int Id { get; set; }

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
    }
}
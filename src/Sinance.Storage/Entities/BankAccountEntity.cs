using Sinance.Communication.Model.BankAccount;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Storage.Entities;

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
    public List<TransactionEntity> Transactions { get; set; }
}
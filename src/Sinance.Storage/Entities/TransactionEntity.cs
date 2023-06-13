using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Storage.Entities;

/// <summary>
/// Transaction entity
/// </summary>
public class TransactionEntity : UserEntityBase
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

    public CategoryEntity Category { get; set; }

    public int? CategoryId { get; set; }

    /// <summary>
    /// Transaction categories
    /// </summary>
    public List<TransactionCategoryEntity> TransactionCategories { get; set; }
}
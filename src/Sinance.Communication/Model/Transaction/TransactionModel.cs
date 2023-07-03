using Sinance.Communication.Model.Category;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.Transaction;

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
    /// Bank account id
    /// </summary>
    public int BankAccountId { get; set; }

    /// <summary>
    /// Categories applied to the transaction
    /// </summary>
    public CategoryModel Category { get; set; }

    /// <summary>
    /// Categories applied to the transaction
    /// </summary>
    public int? CategoryId { get; set; }

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

    public string FromAccount { get; set; }

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
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Storage.Entities;

/// <summary>
/// Transaction category entity
/// </summary>
public class TransactionCategoryEntity : EntityBase
{
    /// <summary>
    /// Amount
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Category
    /// </summary>
    [ForeignKey("CategoryId")]
    public CategoryEntity Category { get; set; }

    /// <summary>
    /// Category id
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Transaction
    /// </summary>
    [ForeignKey("TransactionId")]
    public TransactionEntity Transaction { get; set; }

    /// <summary>
    /// Transaction id
    /// </summary>
    public int TransactionId { get; set; }
}
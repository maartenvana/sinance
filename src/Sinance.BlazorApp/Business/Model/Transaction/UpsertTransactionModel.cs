using System;
using System.ComponentModel.DataAnnotations;

namespace Sinance.BlazorApp.Business.Model.Transaction;

public class UpsertTransactionModel
{
    public int Id { get; set; }
    public string Description { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public decimal Amount { get; set; }
    [Required]
    public DateTime Date { get; set; }
    public string SourceAccountNumber { get; set; }
    public string DestinationAccountNumber { get; set; }
    public int? CategoryId { get; set; }
    [Required]
    public int? BankAccountId { get; set; }
    public bool IsNew => Id == 0;
}

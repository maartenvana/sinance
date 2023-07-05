using System.Collections.Generic;

namespace Sinance.BlazorApp.Business.Model.Transaction;

public class SplitTransactionModel
{
    public int SourceTransactionId { get; set; }
    public List<UpsertTransactionModel> NewTransactions { get; set; } = new List<UpsertTransactionModel>();
}

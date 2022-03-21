using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Business.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.BlazorApp.Components.Categories.Model
{
    public class TransactionsCategoryUpdatedEvent
    {
        public List<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    }
}

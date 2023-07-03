using Sinance.BlazorApp.Business.Model.Transaction;

namespace Sinance.BlazorApp.Model.Events
{
    public class TransactionUpsertedEvent
    {
        public bool Created { get; set; }

        public TransactionModel Transaction { get; set; }
    }
}

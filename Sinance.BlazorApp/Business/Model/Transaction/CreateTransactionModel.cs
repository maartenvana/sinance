using System;

namespace Sinance.BlazorApp.Business.Model.Transaction
{
    public class CreateTransactionModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public int? CategoryId { get; set; }
    }
}

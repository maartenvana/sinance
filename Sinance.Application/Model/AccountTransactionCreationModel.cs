﻿namespace Sinance.Application.Model
{
    public record AccountTransactionCreationModel
    {
        public int BankAccountId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public int? CategoryId { get; set; }
    }
}
namespace Sinance.Domain.Model
{
    public class AccountTransaction : UserEntity
    {
        public string AccountNumber { get; set; }

        public decimal Amount { get; set; }

        public Account BankAccount { get; set; }

        public int BankAccountId { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string DestinationAccount { get; set; }

        public string Name { get; set; }

        public Category Category { get; set; }

        public int? CategoryId { get; set; }

        public ImportTransaction ImportTransaction { get; set; }

        public int? ImportTransactionId { get; set; }

        public AccountTransaction()
        {

        }

        public AccountTransaction(int userId, int bankAccountId, DateTime date, string name, string description, decimal amount, int? categoryId, string account, string destinationAcount, int? importTransactionId)
        {
            UserId = userId;
            BankAccountId = bankAccountId;
            Date = date;
            Name = name;
            Description = description;
            Amount = amount;
            CategoryId = categoryId;
            ImportTransactionId = importTransactionId;
            AccountNumber = account;
            DestinationAccount = destinationAcount;
        }
    }
}

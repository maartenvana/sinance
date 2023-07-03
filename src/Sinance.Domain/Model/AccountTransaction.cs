using Sinance.Domain.Events;

namespace Sinance.Domain.Model
{
    public class AccountTransaction : UserEntity
    {
        public string AccountNumber { get; private set; }

        public decimal Amount { get; private set; }

        public Account BankAccount { get; private set; }

        public int BankAccountId { get; private set; }

        public DateTime Date { get; private set; }

        public string Description { get; private set; }

        public string DestinationAccount { get; private set; }

        public string Name { get; private set; }

        public Category Category { get; private set; }

        public int? CategoryId { get; private set; }

        public ImportTransaction ImportTransaction { get; private set; }

        public int? ImportTransactionId { get; private set; }

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

            AddDomainEvent(new AccountTransactionCreatedDomainEvent(this));
        }

        public void UpdateAmount(decimal newAmount, decimal oldAmount)
        {
            Amount = newAmount;

            AddDomainEvent(new AccountTransactionAmountChangedDomainEvent(this, oldAmount));
        }

        public void UpdateMetadata(DateTime date, string name, string description, int? categoryId, string account, string destinationAcount)
        {
            Date = date;
            Name = name;
            Description = description;
            CategoryId = categoryId;
            AccountNumber = account;
            DestinationAccount = destinationAcount;
        }

        public override void MarkAsDeleted()
        {
            AddDomainEvent(new AccountTransactionDeletedDomainEvent(this));
        }
    }
}

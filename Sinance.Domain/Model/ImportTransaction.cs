namespace Sinance.Domain.Model
{
    public class ImportTransaction : UserEntity
    {
        public string AccountNumber { get; set; }

        public decimal Amount { get; set; }

        public Account BankAccount { get; set; }

        public int BankAccountId { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string DestinationAccount { get; set; }

        public string Name { get; set; }
    }
}

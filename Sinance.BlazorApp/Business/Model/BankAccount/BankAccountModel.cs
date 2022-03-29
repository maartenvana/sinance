using Sinance.Communication.Model.BankAccount;

namespace Sinance.BlazorApp.Business.Model.BankAccount
{
    public class BankAccountModel
    {
        public decimal CurrentBalance { get; set; }
        public int Id { get; set; }

        public string Name { get; set; }
        public BankAccountType Type { get; set; }
        public bool Disabled { get; set; }
        public static BankAccountModel All => new() { Id = -1, Name = "All" };
    }
}
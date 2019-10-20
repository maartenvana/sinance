using Sinance.Communication.Model.BankAccount;
using Sinance.Storage.Entities;

namespace Sinance.Business.Extensions
{
    public static class BankAccountExtensions
    {
        public static BankAccountModel ToDto(this BankAccountEntity bankAccountEntity)
        {
            return new BankAccountModel
            {
                AccountType = bankAccountEntity.AccountType,
                CurrentBalance = bankAccountEntity.CurrentBalance,
                Disabled = bankAccountEntity.Disabled,
                IncludeInProfitLossGraph = bankAccountEntity.IncludeInProfitLossGraph,
                StartBalance = bankAccountEntity.StartBalance,
                Name = bankAccountEntity.Name,
                Id = bankAccountEntity.Id
            };
        }

        public static BankAccountEntity ToNewEntity(this BankAccountModel model, int userId)
        {
            return new BankAccountEntity
            {
                Id = model.Id,
                AccountType = model.AccountType,
                CurrentBalance = model.StartBalance,
                Disabled = model.Disabled,
                IncludeInProfitLossGraph = model.IncludeInProfitLossGraph,
                StartBalance = model.StartBalance,
                Name = model.Name,
                UserId = userId
            };
        }

        public static BankAccountEntity UpdateFromModel(this BankAccountEntity bankAccount, BankAccountModel model)
        {
            bankAccount.Name = model.Name;
            bankAccount.IncludeInProfitLossGraph = model.IncludeInProfitLossGraph;
            bankAccount.StartBalance = model.StartBalance;
            bankAccount.AccountType = model.AccountType;
            bankAccount.Disabled = model.Disabled;

            return bankAccount;
        }
    }
}
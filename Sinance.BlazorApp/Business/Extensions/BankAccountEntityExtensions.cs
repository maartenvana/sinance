using Sinance.BlazorApp.Business.Model.BankAccount;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Extensions
{
    public static class BankAccountEntityExtensions
    {
        public static IEnumerable<BankAccountModel> ToDto(this IEnumerable<BankAccountEntity> entities)
            => entities.Select(x => x.ToDto());

        public static BankAccountModel ToDto(this BankAccountEntity entity)
        {
            return new BankAccountModel
            {
                Id = entity.Id,
                Name = entity.Name,
                CurrentBalance = entity.CurrentBalance.GetValueOrDefault(),
                Type = entity.AccountType,
                Disabled = entity.Disabled
            };
        }
    }
}

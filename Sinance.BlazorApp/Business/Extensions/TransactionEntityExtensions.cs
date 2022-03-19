using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Extensions
{
    public static class TransactionEntityExtensions
    {
        public static IEnumerable<TransactionModel> ToDto(this IEnumerable<TransactionEntity> entities)
            => entities.Select(x => x.ToDto());

        public static TransactionModel ToDto(this TransactionEntity entity)
        {
            return new TransactionModel
            {
                Id = entity.Id,
                Description = entity.Description,
                Name = entity.Name,
                Amount = entity.Amount,
                Date = entity.Date,
                CategoryColorCode = entity.Category?.ColorCode,
                CategoryShortName = entity.Category?.ShortName,
                CategoryId = entity.Category?.Id
            };
        }
    }
}

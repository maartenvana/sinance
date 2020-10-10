using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

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
                Description = entity.Description,
                Name = entity.Name,
                Amount = entity.Amount,
                Date = entity.Date,
                CategoryName = entity.Category?.Name
            };
        }
    }
}

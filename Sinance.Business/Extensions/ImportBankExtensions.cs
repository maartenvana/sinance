using Sinance.Communication.Model.Import;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.Business.Extensions
{
    public static class ImportBankExtensions
    {
        public static IEnumerable<ImportBankModel> ToDto(this IEnumerable<ImportBankEntity> entity) => entity.Select(x => x.ToDto());

        public static ImportBankModel ToDto(this ImportBankEntity entity)
        {
            return new ImportBankModel
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }
}
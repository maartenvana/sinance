using Sinance.Communication.Model.Import;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers;

public interface IBankFileImportHandler
{
    Task<IList<ImportRow>> CreateImportRowsFromFile(IUnitOfWork unitOfWork, Stream fileInputStream, int userId, Guid fileImporterId, int bankAccountId);
    Task<int> SaveImportResultToDatabase(IUnitOfWork unitOfWork, int bankAccountId, int userId, IList<ImportRow> importRows, IList<ImportRow> cachedImportRows);
}
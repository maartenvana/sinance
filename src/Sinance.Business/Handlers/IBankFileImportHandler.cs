using Sinance.Communication.Model.Import;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers;

public interface IBankFileImportHandler
{
    Task<IList<ImportRow>> CreateImportRowsFromFile(SinanceContext context, Stream fileInputStream, int userId, Guid fileImporterId, int bankAccountId);
}
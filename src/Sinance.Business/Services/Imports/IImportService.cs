using Sinance.Communication.Model.Import;
using System.IO;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Imports;

public interface IImportService
{
    Task<ImportModel> CreateImportPreview(Stream fileStream, ImportModel model);

    Task<(int skippedTransactions, int savedTransactions)> SaveImport(ImportModel model);
}
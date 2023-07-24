using Sinance.Communication.Model.Import;
using System.IO;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Imports;

public interface IImportService
{
    Task<(int skippedTransactions, int savedTransactions)> ImportTransactions(Stream fileStream, ImportModel model);
}
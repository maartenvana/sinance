using Sinance.Communication.Model.CustomReport;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services
{
    public interface ICustomReportService
    {
        Task<CustomReportModel> GetCustomReportByIdForCurrentUser(int customReportId);

        Task<IEnumerable<CustomReportModel>> GetCustomReportsForCurrentUser();
    }
}
using Sinance.Communication.Model.CustomReport;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services;

public interface ICustomReportService
{
    Task<CustomReportModel> CreateCustomReport(CustomReportModel model);

    Task<CustomReportModel> GetCustomReportByIdForCurrentUser(int customReportId);

    Task<List<CustomReportModel>> GetCustomReportsForCurrentUser();

    Task<CustomReportModel> UpdateCustomReport(CustomReportModel model);
}
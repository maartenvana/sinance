using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services
{
    public interface ICustomReportService
    {
        Task<IList<CustomReportEntity>> GetCustomReportsForCurrentUser();
    }
}
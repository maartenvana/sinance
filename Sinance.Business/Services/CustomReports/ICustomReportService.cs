using Sinance.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services
{
    public interface ICustomReportService
    {
        Task<IList<CustomReport>> GetCustomReportsForCurrentUser();
    }
}
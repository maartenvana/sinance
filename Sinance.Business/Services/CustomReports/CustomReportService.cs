using Sinance.Domain.Entities;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Business.Services.Authentication;

namespace Sinance.Business.Services
{
    public class CustomReportService : ICustomReportService
    {
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CustomReportService(Func<IUnitOfWork> unitOfWork, IAuthenticationService sessionService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Custom reports active in this session
        /// </summary>
        public async Task<IList<CustomReport>> GetCustomReportsForCurrentUser()
        {
            var userId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var customReports = (await unitOfWork.CustomReportRepository
                .FindAll(item => item.UserId == userId))
                .OrderBy(item => item.Name)
                .ToList();

            return customReports;
        }
    }
}
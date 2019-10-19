using Sinance.Storage.Entities;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.CustomReport;
using Sinance.Business.Extensions;
using Sinance.Business.Exceptions;

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

        public async Task<CustomReportModel> GetCustomReportByIdForCurrentUser(int customReportId)
        {
            var userId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var customReport = await unitOfWork.CustomReportRepository
                .FindSingle(
                    findQuery: item => item.UserId == userId,
                    includeProperties: new string[] {
                        nameof(CustomReportEntity.ReportCategories),
                        $"{nameof(CustomReportEntity.ReportCategories)}.{nameof(CustomReportCategoryEntity.Category)}"
                    });

            if (customReport == null)
            {
                throw new NotFoundException(nameof(CustomReportEntity));
            }

            return customReport.ToDto();
        }

        /// <summary>
        /// Custom reports active in this session
        /// </summary>
        public async Task<IEnumerable<CustomReportModel>> GetCustomReportsForCurrentUser()
        {
            var userId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var customReports = (await unitOfWork.CustomReportRepository
                .FindAll(
                    findQuery: item => item.UserId == userId,
                    includeProperties: new string[] {
                        nameof(CustomReportEntity.ReportCategories),
                        $"{nameof(CustomReportEntity.ReportCategories)}.{nameof(CustomReportCategoryEntity.Category)}"
                    }))
                .OrderBy(item => item.Name)
                .ToDto();

            return customReports;
        }
    }
}
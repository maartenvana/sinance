using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.CustomReport;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<CustomReportModel> CreateCustomReport(CustomReportModel model)
        {
            var userId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            await ValidateModelCategories(model, userId, unitOfWork);

            var entity = model.ToNewEntity(userId);

            unitOfWork.CustomReportRepository.Insert(entity);
            await unitOfWork.SaveAsync();

            // Reselect to get the categories included
            entity = await FindCustomReportWithCategories(userId, unitOfWork);

            return entity.ToDto();
        }

        public async Task<CustomReportModel> GetCustomReportByIdForCurrentUser(int customReportId)
        {
            var userId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var customReport = await FindCustomReportWithCategories(userId, unitOfWork);

            if (customReport == null)
            {
                throw new NotFoundException(nameof(CustomReportEntity));
            }

            return customReport.ToDto();
        }

        /// <summary>
        /// Custom reports active in this session
        /// </summary>
        public async Task<List<CustomReportModel>> GetCustomReportsForCurrentUser()
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
                .ToList()
                .ToDto();

            return customReports;
        }

        public async Task<CustomReportModel> UpdateCustomReport(CustomReportModel model)
        {
            var userId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            await ValidateModelCategories(model, userId, unitOfWork);

            var entity = await FindCustomReportWithCategories(userId, unitOfWork);

            entity.UpdateWithModel(model);
            await unitOfWork.SaveAsync();

            // Reselect to get the categories included
            entity = await FindCustomReportWithCategories(userId, unitOfWork);

            return entity.ToDto();
        }

        private static async Task<CustomReportEntity> FindCustomReportWithCategories(int userId, IUnitOfWork unitOfWork)
        {
            var report = await unitOfWork.CustomReportRepository
                            .FindSingleTracked(
                                findQuery: item => item.UserId == userId,
                                includeProperties: new string[] {
                                    nameof(CustomReportEntity.ReportCategories),
                                    $"{nameof(CustomReportEntity.ReportCategories)}.{nameof(CustomReportCategoryEntity.Category)}"
                                });

            if (report == null)
            {
                throw new NotFoundException(nameof(CustomReportEntity));
            }

            return report;
        }

        private static async Task ValidateModelCategories(CustomReportModel model, int userId, IUnitOfWork unitOfWork)
        {
            // Validate categories
            var allCategories = await unitOfWork.CategoryRepository.FindAll(x => x.UserId == userId);
            var allValid = model.Categories.All(x => allCategories.Any(y => y.Id == x.CategoryId));

            if (!allValid)
            {
                throw new NotFoundException(nameof(CategoryEntity));
            }
        }
    }
}
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
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
        private readonly Func<IUnitOfWork> _unitOfWork;
        private readonly IUserIdProvider _userIdProvider;

        public CustomReportService(
            Func<IUnitOfWork> unitOfWork,
            IUserIdProvider userIdProvider)
        {
            _unitOfWork = unitOfWork;
            _userIdProvider = userIdProvider;
        }

        public async Task<CustomReportModel> CreateCustomReport(CustomReportModel model)
        {
            using var unitOfWork = _unitOfWork();

            await ValidateModelCategories(model, unitOfWork);

            var entity = model.ToNewEntity(_userIdProvider.GetCurrentUserId());

            unitOfWork.CustomReportRepository.Insert(entity);
            await unitOfWork.SaveAsync();

            // Reselect to get the categories included
            entity = await FindCustomReportWithCategories(entity.Id, unitOfWork);

            return entity.ToDto();
        }

        public async Task<CustomReportModel> GetCustomReportByIdForCurrentUser(int customReportId)
        {
            using var unitOfWork = _unitOfWork();

            var customReport = await FindCustomReportWithCategories(customReportId, unitOfWork);

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
            using var unitOfWork = _unitOfWork();
            var customReports = (await unitOfWork.CustomReportRepository
                .ListAll(
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
            using var unitOfWork = _unitOfWork();

            await ValidateModelCategories(model, unitOfWork);

            var entity = await FindCustomReportWithCategories(model.Id, unitOfWork);

            entity.UpdateWithModel(model);
            await unitOfWork.SaveAsync();

            // Reselect to get the categories included
            entity = await FindCustomReportWithCategories(model.Id, unitOfWork);

            return entity.ToDto();
        }

        private static async Task<CustomReportEntity> FindCustomReportWithCategories(int reportId, IUnitOfWork unitOfWork)
        {
            var report = await unitOfWork.CustomReportRepository
                            .FindSingleTracked(
                                findQuery: x => x.Id == reportId,
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

        private static async Task ValidateModelCategories(CustomReportModel model, IUnitOfWork unitOfWork)
        {
            // Validate categories
            var allCategories = await unitOfWork.CategoryRepository.ListAll();
            var allValid = model.Categories.All(x => allCategories.Any(y => y.Id == x.CategoryId));

            if (!allValid)
            {
                throw new NotFoundException(nameof(CategoryEntity));
            }
        }
    }
}
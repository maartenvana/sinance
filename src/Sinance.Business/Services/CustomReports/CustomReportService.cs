using Microsoft.EntityFrameworkCore;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.CustomReport;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services;

public class CustomReportService : ICustomReportService
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;
    private readonly IUserIdProvider _userIdProvider;

    public CustomReportService(
        IDbContextFactory<SinanceContext> dbContextFactory,
        IUserIdProvider userIdProvider)
    {
        _dbContextFactory = dbContextFactory;
        _userIdProvider = userIdProvider;
    }

    public async Task<CustomReportModel> CreateCustomReport(CustomReportModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

        await ValidateModelCategories(model, context);

        var entity = model.ToNewEntity(_userIdProvider.GetCurrentUserId());

        await context.CustomReports.AddAsync(entity);
        await context.SaveChangesAsync();

        // Reselect to get the categories included
        entity = await FindCustomReportWithCategories(entity.Id, context);

        return entity.ToDto();
    }

    public async Task<CustomReportModel> GetCustomReportByIdForCurrentUser(int customReportId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var customReport = await FindCustomReportWithCategories(customReportId, context);

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
        using var context = _dbContextFactory.CreateDbContext();

        var customReports = await context.CustomReports
            .Include(x => x.ReportCategories).ThenInclude(x => x.Category)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return customReports.ToDto();
    }

    public async Task<CustomReportModel> UpdateCustomReport(CustomReportModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

        await ValidateModelCategories(model, context);

        var entity = await FindCustomReportWithCategories(model.Id, context);

        entity.UpdateWithModel(model);
        await context.SaveChangesAsync();

        // Reselect to get the categories included
        entity = await FindCustomReportWithCategories(model.Id, context);

        return entity.ToDto();
    }

    private static async Task<CustomReportEntity> FindCustomReportWithCategories(int reportId, SinanceContext sinanceContext)
    {
        var report = await sinanceContext.CustomReports
            .Include(x => x.ReportCategories).ThenInclude(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == reportId);

        if (report == null)
            throw new NotFoundException(nameof(CustomReportEntity));

        return report;
    }

    private static async Task ValidateModelCategories(CustomReportModel model, SinanceContext context)
    {
        // Validate categories
        var allCategories = await context.Categories.ToListAsync();
        var allValid = model.Categories.All(x => allCategories.Any(y => y.Id == x.CategoryId));

        if (!allValid)
        {
            throw new NotFoundException(nameof(CategoryEntity));
        }
    }
}
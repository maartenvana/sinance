using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Storage;
using Sinance.Storage;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Services;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<SinanceContext> dbContextFactory;

    public CategoryService(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public List<CategoryModel> GetAllCategories()
    {
        using var context = this.dbContextFactory.CreateDbContext();

        var bankAccountEntities = context.Categories.ToList();

        return bankAccountEntities.ToDto().ToList();
    }
}
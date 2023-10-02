using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Exceptions;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.CategoryMappings;
using Sinance.Communication;
using Sinance.Communication.Model.CategoryMapping;
using Sinance.Communication.Model.Import;
using Sinance.Web;
using Sinance.Web.Helper;
using Sinance.Web.Model;
using System.Threading.Tasks;

namespace Sinance.Controllers;

/// <summary>
/// Category Mapping controller
/// </summary>
[Authorize]
public class CategoryMappingController : Controller
{
    private readonly ICategoryMappingService _categoryMappingService;
    private readonly ICategoryService _categoryService;

    public CategoryMappingController(
        ICategoryService categoryService,
        ICategoryMappingService categoryMappingService)
    {
        _categoryService = categoryService;
        _categoryMappingService = categoryMappingService;
    }

    /// <summary>
    /// Starts the add process for a new categorymapping
    /// </summary>
    /// <param name="categoryId">Id of the category to add the mapping to</param>
    /// <returns>Partial view for adding the mapping</returns>
    public async Task<IActionResult> AddCategoryMapping(int categoryId)
    {
        try
        {
            var categoryModel = await _categoryService.GetCategoryByIdForCurrentUser(categoryId);

            return PartialView("UpsertCategoryMapping", new CategoryMappingModel
            {
                CategoryName = categoryModel.Name,
                CategoryId = categoryModel.Id
            });
        }
        catch (NotFoundException)
        {
            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
            return View("UpsertCategoryMapping");
        }
    }

    /// <summary>
    /// Starts the process of editing a category mapping
    /// </summary>
    /// <param name="categoryMappingId">Category mapping to edit</param>
    /// <returns>Partial view to edit the mapping</returns>
    public async Task<IActionResult> EditCategoryMapping(int categoryMappingId)
    {
        try
        {
            var model = await _categoryMappingService.GetCategoryMappingByIdForCurrentUser(categoryMappingId);
            return PartialView("UpsertCategoryMapping", model);
        }
        catch (NotFoundException)
        {
            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryMappingNotFound);
            return View("UpsertCategoryMapping");
        }
    }

    /// <summary>
    /// Removes a category mapping from the database
    /// </summary>
    /// <param name="categoryMappingId">Id of the mapping to remove</param>
    /// <returns>Redirect to the edit category view</returns>
    public async Task<IActionResult> RemoveCategoryMapping(int categoryMappingId, int categoryId)
    {
        try
        {
            await _categoryMappingService.DeleteCategoryMappingByIdForCurrentUser(categoryMappingId);

            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryMappingDeleted);
            return RedirectToAction("EditCategory", "Category", new { categoryId });
        }
        catch (NotFoundException)
        {
            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryMappingNotFound);
            return RedirectToAction("Index", "Category");
        }
    }

    /// <summary>
    /// Upserts a category mapping to the database
    /// </summary>
    /// <param name="model">Model to upsert</param>
    /// <returns>Result of the upsert</returns>
    public async Task<IActionResult> UpsertCategoryMapping(CategoryMappingModel model)
    {
        if (ModelState.IsValid)
        {
            // Placeholder extra validation, in the future all mappings should be possible
            if (model.ColumnTypeId != ColumnType.Description &&
                model.ColumnTypeId != ColumnType.Name &&
                model.ColumnTypeId != ColumnType.DestinationAccount)
            {
                ModelState.AddModelError("", Resources.UnsupportedColumnTypeMapping);
                return PartialView("UpsertCategoryMapping", model);
            }

            if (model.Id > 0)
            {
                try
                {
                    await _categoryMappingService.UpdateCategoryMappingForCurrentUser(model);

                    return Json(new SinanceJsonResult
                    {
                        Success = true
                    });
                }
                catch (NotFoundException)
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryMappingNotFound);
                    return PartialView("UpsertCategoryMapping", model);
                }
            }
            else
            {
                try
                {
                    await _categoryMappingService.CreateCategoryMappingForCurrentUser(model);

                    return Json(new SinanceJsonResult
                    {
                        Success = true
                    });
                }
                catch (NotFoundException)
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
                    return PartialView("UpsertCategoryMapping", model);
                }
            }
        }

        return PartialView("UpsertCategoryMapping", model);
    }
}
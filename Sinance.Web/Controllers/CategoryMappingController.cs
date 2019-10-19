using Sinance.Business.Classes;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.CategoryMappings;
using Sinance.Business.Exceptions;
using Sinance.Communication.Import;
using Sinance.Communication.CategoryMapping;

namespace Sinance.Controllers
{
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
                var categoryModel = _categoryService.GetCategoryByIdForCurrentUser(categoryId);

                return PartialView("UpsertCategoryMapping", categoryModel);
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
                return View("Index");
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
                return View("Index");
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
                        var updatedModel = await _categoryMappingService.UpdateCategoryMappingForCurrentUser(model);

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
                        var createdModel = await _categoryMappingService.CreateCategoryMappingForCurrentUser(model);

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
}
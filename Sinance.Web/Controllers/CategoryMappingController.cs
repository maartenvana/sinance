using Sinance.Business.Classes;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Controllers
{
    /// <summary>
    /// Category Mapping controller
    /// </summary>
    [Authorize]
    public class CategoryMappingController : Controller
    {
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CategoryMappingController(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService sessionService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Starts the add process for a new categorymapping
        /// </summary>
        /// <param name="categoryId">Id of the category to add the mapping to</param>
        /// <returns>Partial view for adding the mapping</returns>
        public async Task<IActionResult> AddCategoryMapping(int categoryId)
        {
            if (categoryId < 0)
                throw new ArgumentOutOfRangeException(nameof(categoryId));

            CategoryMappingModel model = null;
            var currentUserId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var category = await unitOfWork.CategoryRepository.FindSingle(item => item.Id == categoryId && item.UserId == currentUserId);
            if (category != null)
            {
                model = new CategoryMappingModel
                {
                    CategoryName = category.Name,
                    CategoryId = categoryId
                };
            }
            else
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
            }

            return PartialView("UpsertCategoryMapping", model);
        }

        /// <summary>
        /// Starts the process of editing a category mapping
        /// </summary>
        /// <param name="categoryMappingId">Category mapping to edit</param>
        /// <returns>Partial view to edit the mapping</returns>
        public async Task<IActionResult> EditCategoryMapping(int categoryMappingId)
        {
            if (categoryMappingId < 0)
                throw new ArgumentOutOfRangeException(nameof(categoryMappingId));

            var currentUserId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var existingMapping = await unitOfWork.CategoryMappingRepository.FindSingle(item => item.Id == categoryMappingId &&
                                       item.Category.UserId == currentUserId,
                                       includeProperties: nameof(CategoryMapping.Category));
            CategoryMappingModel model = null;

            if (existingMapping != null)
            {
                model = CategoryMappingModel.CreateCategoryMappingModel(existingMapping);
            }
            else
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryMappingNotFound);
            }

            return PartialView("UpsertCategoryMapping", model);
        }

        /// <summary>
        /// Removes a category mapping from the database
        /// </summary>
        /// <param name="categoryMappingId">Id of the mapping to remove</param>
        /// <returns>Redirect to the edit category view</returns>
        public async Task<IActionResult> RemoveCategoryMapping(int categoryMappingId)
        {
            if (categoryMappingId < 0)
                throw new ArgumentOutOfRangeException(nameof(categoryMappingId));

            var currentUserId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var mapping = await unitOfWork.CategoryMappingRepository.FindSingleTracked(item => item.Id == categoryMappingId &&
                               item.Category.UserId == currentUserId);

            if (mapping != null)
            {
                unitOfWork.CategoryMappingRepository.Delete(mapping);
                await unitOfWork.SaveAsync();

                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryMappingDeleted);
                return RedirectToAction("EditCategory", "Category", new { categoryId = mapping.CategoryId });
            }
            else
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
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (ModelState.IsValid)
            {
                var currentUserId = await _sessionService.GetCurrentUserId();

                using var unitOfWork = _unitOfWork();

                // Placeholder extra validation, in the future all mappings should be possible
                if (model.ColumnTypeId != ColumnType.Description &&
                    model.ColumnTypeId != ColumnType.Name &&
                    model.ColumnTypeId != ColumnType.DestinationAccount)
                {
                    ModelState.AddModelError("", Resources.UnsupportedColumnTypeMapping);
                    return PartialView("UpsertCategoryMapping", model);
                }
                else if (unitOfWork.CategoryRepository.FindSingleTracked(item => item.UserId == currentUserId &&
                                                                        item.Id == model.CategoryId) == null)
                {
                    ModelState.AddModelError("", Resources.CategoryNotFound);
                    return PartialView("UpsertCategoryMapping", model);
                }
                else
                {
                    if (model.Id > 0)
                    {
                        var existingCategoryMapping = await unitOfWork.CategoryMappingRepository.FindSingleTracked(item => item.Id == model.Id &&
                                                                                                                item.Category.UserId == currentUserId);
                        if (existingCategoryMapping != null)
                        {
                            existingCategoryMapping.Update(matchValue: model.MatchValue,
                                columnType: model.ColumnTypeId,
                                categoryId: model.CategoryId);

                            unitOfWork.CategoryMappingRepository.Update(existingCategoryMapping);
                            await unitOfWork.SaveAsync();

                            return Json(new SinanceJsonResult
                            {
                                Success = true
                            });
                        }
                        else
                        {
                            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryMappingNotFound);
                            return PartialView("UpsertCategoryMapping", model);
                        }
                    }
                    else
                    {
                        var newCategoryMapping = new CategoryMapping();
                        newCategoryMapping.Update(matchValue: model.MatchValue,
                            columnType: model.ColumnTypeId,
                            categoryId: model.CategoryId);

                        unitOfWork.CategoryMappingRepository.Insert(newCategoryMapping);
                        await unitOfWork.SaveAsync();

                        return Json(new SinanceJsonResult
                        {
                            Success = true
                        });
                    }
                }
            }

            return PartialView("UpsertCategoryMapping", model);
        }
    }
}
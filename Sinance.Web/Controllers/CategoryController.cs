using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sinance.Business.Exceptions;
using Sinance.Business.Services.Categories;
using Sinance.Communication.Model.Category;
using Sinance.Web;
using Sinance.Web.Helper;
using Sinance.Web.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Controllers
{
    /// <summary>
    /// Category controller
    /// </summary>
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Provides an actionresult for adding a new category
        /// </summary>
        /// <returns>Actionresult for adding a new category</returns>
        public async Task<IActionResult> AddCategory()
        {
            var model = new UpsertCategoryModel
            {
                AvailableParentCategories = await CreateAvailableParentCategoriesSelectList(new CategoryModel()),
                CategoryModel = new CategoryModel()
            };

            return View("UpsertCategory", model);
        }

        /// <summary>
        /// Provides an actionresult for editing a new category
        /// </summary>
        /// <param name="categoryId">Id of category to edit</param>
        /// <param name="includeTransactions">Include transactions or not for overview</param>
        /// <returns>Actionresult for editing a new category</returns>
        public async Task<IActionResult> EditCategory(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdForCurrentUser(categoryId);
                var availableParentCategories = await CreateAvailableParentCategoriesSelectList(category);

                var model = new UpsertCategoryModel
                {
                    CategoryModel = category,
                    AvailableParentCategories = availableParentCategories
                };

                return View("UpsertCategory", model);
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Default index page
        /// </summary>
        /// <returns>Index view</returns>
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesForCurrentUser();

            return View("Index", categories);
        }

        /// <summary>
        /// Removes the given category from the database
        /// </summary>
        /// <param name="categoryId">Id of category to remove</param>
        /// <returns>Index page</returns>
        public async Task<IActionResult> RemoveCategory(int categoryId)
        {
            try
            {
                await _categoryService.DeleteCategoryByIdForCurrentUser(categoryId);
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryRemoved);
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
            }
            catch (DeleteStandardCategoryException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.DeleteStandardCategoryExceptionMessage);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Assigns the category to the previously previewed transactions
        /// </summary>
        /// <param name="categoryId">Id of the category to assign</param>
        /// <returns>Redirect to the edit category action</returns>
        public async Task<IActionResult> UpdateCategoryToMappedTransactions(int categoryId, IEnumerable<int> transactionIds)
        {
            try
            {
                await _categoryService.MapCategoryToTransactionsForCurrentUser(categoryId, transactionIds);

                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryMappingsAppliedToTransactions);

                return RedirectToAction("EditCategory", new { categoryId });
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Upserts the category model to the database
        /// </summary>
        /// <param name="model">Model to upsert</param>
        /// <returns>Index page and message if success</returns>
        public async Task<IActionResult> UpsertCategory(UpsertCategoryModel model)
        {
            if (model.CategoryModel.Id > 0)
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction("EditCategory", new { model.CategoryModel.Id });
                }

                try
                {
                    await _categoryService.UpdateCategoryForCurrentUser(model.CategoryModel);
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryUpdated);
                }
                catch (NotFoundException)
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
                }
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction("AddCategory");
                }

                try
                {
                    await _categoryService.CreateCategoryForCurrentUser(model.CategoryModel);
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryCreated);
                }
                catch (AlreadyExistsException)
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, string.Format(Resources.CategoryAlreadyExists, model.CategoryModel.Name));
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Creates a list of available parent categories to link with
        /// </summary>
        /// <param name="category">Current category to get possible parent categories for</param>
        /// <returns>List of available categories</returns>
        internal async Task<List<SelectListItem>> CreateAvailableParentCategoriesSelectList(CategoryModel category)
        {
            var categories = await _categoryService.GetPossibleParentCategoriesForCurrentUser(category.Id);

            var availableCategories = new List<SelectListItem>{
                    new SelectListItem {
                        Text = Resources.NoMainCategory,
                        Value = "0",
                        Selected = category.ParentId == null
                    }
                };

            availableCategories.AddRange(categories.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(CultureInfo.InvariantCulture),
                Text = item.Name,
                Selected = item.Id == category.ParentId
            }));

            return availableCategories.ToList();
        }
    }
}
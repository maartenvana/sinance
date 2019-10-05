using Sinance.Business.Handlers;
using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Controllers
{
    /// <summary>
    /// Category controller
    /// </summary>
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CategoryController(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService sessionService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Provides an actionresult for adding a new category
        /// </summary>
        /// <returns>Actionresult for adding a new category</returns>
        public async Task<IActionResult> AddCategory()
        {
            CategoryModel model = new CategoryModel
            {
                AvailableCategories = await AvailableParentCategories(new Category())
            };

            return View("UpsertCategory", model);
        }

        /// <summary>
        /// Provides an actionresult for editing a new category
        /// </summary>
        /// <param name="categoryId">Id of category to edit</param>
        /// <param name="includeTransactions">Include transactions or not for overview</param>
        /// <returns>Actionresult for editing a new category</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Needed for optional parameter from outside")]
        public async Task<IActionResult> EditCategory(int categoryId, bool includeTransactions = false)
        {
            if (categoryId < 0)
                throw new ArgumentOutOfRangeException(nameof(categoryId));

            ActionResult result;
            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                Category category = unitOfWork.CategoryRepository.FindSingleTracked(item => item.Id == categoryId &&
                                                                        item.UserId == currentUserId,
                                                                        "ParentCategory", "ChildCategories", "CategoryMappings");

                if (category != null)
                {
                    IEnumerable<SelectListItem> availableCategories = await AvailableParentCategories(category);

                    CategoryModel model = CategoryModel.CreateCategoryModel(category);
                    model.AvailableCategories = availableCategories;

                    if (includeTransactions)
                        model.AutomaticMappings = (await CreateMappingPreview(category)).OrderByDescending(item => item.Key.Date).ToList();

                    result = View("UpsertCategory", model);
                }
                else
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
                    result = RedirectToAction("Index");
                }

                return result;
            }
        }

        /// <summary>
        /// Default index page
        /// </summary>
        /// <returns>Index view</returns>
        public async Task<IActionResult> Index()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                IList<Category> categories = unitOfWork.CategoryRepository.FindAllTracked(item => item.UserId == currentUserId);

                var regularCategories = categories.Where(x => x.IsRegular || x.ChildCategories.Any(y => y.IsRegular) || x.ParentCategory?.IsRegular == true).ToList();
                var irregularCategories = categories.Where(x => (x.ParentCategory?.IsRegular != true && !x.IsRegular) || (!x.IsRegular && x.ChildCategories.Any(y => !y.IsRegular))).ToList();

                return View("Index", new CategoriesOverviewModel
                {
                    RegularCategories = regularCategories,
                    IrregularCategories = irregularCategories
                });
            }
        }

        /// <summary>
        /// Removes the given category from the database
        /// </summary>
        /// <param name="categoryId">Id of category to remove</param>
        /// <returns>Index page</returns>
        public async Task<IActionResult> RemoveCategory(int categoryId)
        {
            if (categoryId < 0)
                throw new ArgumentOutOfRangeException(nameof(categoryId));

            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                Category category = unitOfWork.CategoryRepository.FindSingleTracked(item => item.Id == categoryId && item.UserId == currentUserId);
                var transactionCategories = unitOfWork.TransactionCategoryRepository.FindAllTracked(x => x.CategoryId == categoryId);

                if (category != null)
                {
                    unitOfWork.TransactionCategoryRepository.DeleteRange(transactionCategories);
                    unitOfWork.CategoryRepository.Delete(category);

                    await unitOfWork.SaveAsync();
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryRemoved);
                }
                else
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error,
                        ViewBag.Message = Resources.CategoryNotFound);

                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Assigns the category to the previously previewed transactions
        /// </summary>
        /// <param name="categoryId">Id of the category to assign</param>
        /// <returns>Redirect to the edit category action</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Method is fine")]
        public async Task<IActionResult> UpdateCategoryToMappedTransactions(int categoryId)
        {
            if (categoryId < 0)
                throw new ArgumentOutOfRangeException(nameof(categoryId));

            var currentUserId = await _sessionService.GetCurrentUserId();

            IActionResult result;

            using (var unitOfWork = _unitOfWork())
            {
                Category category = unitOfWork.CategoryRepository.FindSingleTracked(item => item.Id == categoryId && item.UserId == currentUserId, "CategoryMappings");

                if (category != null)
                {
                    IList<Transaction> transactions = unitOfWork.TransactionRepository.FindAllTracked(item => item.UserId == currentUserId);
                    IEnumerable<Transaction> mappedTransactions = CategoryHandler.PreviewMapTransactions(category.CategoryMappings, transactions).ToList();

                    await CategoryHandler.MapCategoryToTransactions(unitOfWork, categoryId, mappedTransactions);

                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success,
                        ViewBag.Message = Resources.CategoryMappingsAppliedToTransactions);

                    result = RedirectToAction("EditCategory", new { categoryId });
                }
                else
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error,
                        ViewBag.Message = Resources.CategoryNotFound);
                    result = RedirectToAction("Index");
                }

                return result;
            }
        }

        /// <summary>
        /// Upserts the category model to the database
        /// </summary>
        /// <param name="model">Model to upsert</param>
        /// <returns>Index page and message if success</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Multiple exceptions types all require the same handling")]
        public async Task<IActionResult> UpsertCategory(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            ActionResult result;

            if (ModelState.IsValid)
            {
                var currentUserId = await _sessionService.GetCurrentUserId();

                // Set the parent id to null if 0 (otherwise it does not pass client side validation
                try
                {
                    using (var unitOfWork = _unitOfWork())
                    {
                        if (model.Id > 0)
                        {
                            var existingCategory = unitOfWork.CategoryRepository.FindSingleTracked(item => item.Id == model.Id && item.UserId == currentUserId);
                            if (existingCategory != null)
                            {
                                existingCategory.Update(name: model.Name,
                                    colorCode: model.ColorCode,
                                    parentId: model.ParentId.GetValueOrDefault() == 0 ? null : model.ParentId,
                                    isRegular: model.IsRegular);

                                unitOfWork.CategoryRepository.Update(existingCategory);
                                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryUpdated);
                            }
                            else
                                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.CategoryNotFound);
                        }
                        else
                        {
                            Category newCategory = new Category();
                            newCategory.Update(name: model.Name,
                                colorCode: model.ColorCode,
                                parentId: model.ParentId.GetValueOrDefault() == 0 ? null : model.ParentId,
                                isRegular: model.IsRegular);

                            newCategory.UserId = currentUserId;

                            unitOfWork.CategoryRepository.Insert(newCategory);
                            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.CategoryCreated);
                        }

                        await unitOfWork.SaveAsync();
                        result = RedirectToAction("Index");
                    }
                }
                catch (Exception)
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.Error);
                    result = View(model);
                }
            }
            else
                result = View(model);

            return result;
        }

        /// <summary>
        /// Creates a list of available parent categories to link with
        /// </summary>
        /// <param name="category">Current category to get possible parent categories for</param>
        /// <returns>List of available categories</returns>
        internal async Task<IEnumerable<SelectListItem>> AvailableParentCategories(Category category)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                // Only select category that arent linked to a parent, no sub categories for recursive linking
                List<Category> categories = unitOfWork.CategoryRepository.FindAllTracked(item => item.ParentId == null &&
                                                                            item.Id != category.Id &&
                                                                            item.UserId == currentUserId).ToList();

                List<SelectListItem> availableCategories = new List<SelectListItem>{
                    new SelectListItem {
                        Text = Resources.NoMainCategory,
                        Value = "0",
                        Selected = category.ParentId == null
                    }
                };

                availableCategories.AddRange(categories.ConvertAll(item => new SelectListItem
                {
                    Value = item.Id.ToString(CultureInfo.InvariantCulture),
                    Text = item.Name,
                    Selected = item.Id == category.ParentId
                }));

                return availableCategories;
            }
        }

        /// <summary>
        /// Creates a mapping preview for categories
        /// </summary>
        /// <param name="category">Category to preview</param>
        /// <returns>Collection of transactions</returns>
        private async Task<IEnumerable<KeyValuePair<Transaction, bool>>> CreateMappingPreview(Category category)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                IEnumerable<Transaction> allTransactions = unitOfWork.TransactionRepository.FindAllTracked(item => item.UserId == currentUserId);

                IList<KeyValuePair<Transaction, bool>> transactions =
                    CategoryHandler.PreviewMapTransactions(category.CategoryMappings, allTransactions).ToList().ConvertAll(item => new KeyValuePair<Transaction, bool>(item, true));

                foreach (Transaction transaction in
                allTransactions.Except(transactions.Select(item => item.Key))
                    .Where(item => item.TransactionCategories.Any(categoryItem => categoryItem.CategoryId == category.Id)))
                {
                    transactions.Add(new KeyValuePair<Transaction, bool>(transaction, false));
                }
                return transactions;
            }
        }
    }
}
using Sinance.Domain.Entities;
using Sinance.Web.Model.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Web.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public BudgetController(
            IAuthenticationService sessionService,
            Func<IUnitOfWork> unitOfWork)
        {
            _sessionService = sessionService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var categories = await unitOfWork.CategoryRepository.FindAll(x => x.UserId == currentUserId);

            var model = new CreateBudgetModel
            {
                AvailableCategories = categories
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetModel model)
        {
            using var unitOfWork = _unitOfWork();
            unitOfWork.BudgetRepository.Insert(new Budget
            {
                CategoryId = model.SelectedCategoryId,
                Amount = model.Amount
            });

            await unitOfWork.SaveAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            using var unitOfWork = _unitOfWork();
            var budget = await unitOfWork.BudgetRepository.FindSingle(x => x.Id == id);
            unitOfWork.BudgetRepository.Delete(budget);
            await unitOfWork.SaveAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            using var unitOfWork = _unitOfWork();
            var budget = await unitOfWork.BudgetRepository.FindSingle(x => x.Id == id);

            var model = new EditBudgetModel
            {
                BudgetId = budget.Id,
                Amount = budget.Amount.GetValueOrDefault()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditBudgetModel model)
        {
            using var unitOfWork = _unitOfWork();
            var budget = await unitOfWork.BudgetRepository.FindSingleTracked(x => x.Id == model.BudgetId);
            budget.Amount = model.Amount;

            unitOfWork.BudgetRepository.Update(budget);

            await unitOfWork.SaveAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var budgets = await unitOfWork.BudgetRepository.ListAll(nameof(Budget.Category), $"{nameof(Budget.Category)}.{nameof(Budget.Category.ChildCategories)}");

            var model = new BudgetIndexModel
            {
                Budgets = budgets
                    .OrderBy(x => x.Category.Name)
                    .ThenBy(x => x.Category.ParentCategory?.Name)
                    .Select(x => new BudgetModel
                    {
                        Amount = x.Amount.GetValueOrDefault(),
                        CategoryName = x.Category.Name,
                        ParentCategoryName = x.Category.ParentCategory?.Name,
                        CategoryId = x.CategoryId,
                        ParentCategoryId = x.Category.ParentCategory?.Id,
                        Id = x.Id
                    }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Status(DateTime? date)
        {
            var dateToUse = date ?? DateTime.Now;

            return await Status(month: dateToUse.Month, year: dateToUse.Year);
        }

        public async Task<IActionResult> Status(int month, int year)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var budgets = await unitOfWork.BudgetRepository.ListAll(nameof(Budget.Category), $"{nameof(Budget.Category)}.{nameof(Budget.Category.ChildCategories)}");

            var transactions = await unitOfWork.TransactionRepository.FindAll(item =>
               item.Date.Month == month &&
               item.Date.Year == year &&
               item.UserId == currentUserId &&
               item.TransactionCategories.Any(x => budgets.Any(y => y.CategoryId == x.CategoryId) ||
                                                   budgets.Any(y => y.Category.ChildCategories.Any(z => z.Id == x.CategoryId))),
                includeProperties: new string[] { nameof(Transaction.TransactionCategories), "TransactionCategories.Category", "TransactionCategories.Category.ChildCategories" });

            var model = new BudgetStatusModel
            {
                StatusDate = new DateTime(year: year, month: month, day: 1)
            };

            foreach (var budget in budgets)
            {
                var amount = transactions.Where(transaction =>
                    transaction.TransactionCategories.Any(x => budgets.Any(y => y.CategoryId == x.CategoryId) ||
                                                                budgets.Any(y => y.Category.ChildCategories.Any(z => z.Id == x.CategoryId)))).Sum(x => x.Amount * -1);

                model.BudgetStatus.Add(new BudgetModel
                {
                    Amount = budget.Amount.GetValueOrDefault(),
                    CategoryName = budget.Category.Name,
                    ParentCategoryName = budget.Category.ParentCategory?.Name,
                    CategoryId = budget.CategoryId,
                    ParentCategoryId = budget.Category.ParentCategory?.Id,
                    Id = budget.Id
                }, amount);
            }
            return View(model);
        }
    }
}
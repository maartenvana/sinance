using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Finances.Bll.Handlers;
using Finances.Controllers;
using Finances.Domain.Entities;
using Finances.Models;
using Finances.UnitTestBase.Classes;
using Rhino.Mocks;
using NUnit.Framework;

namespace Finances.Web.Tests.Controllers
{
    /// <summary>
    /// Test class for the category controller
    /// </summary>
    [TestFixture,
    SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Types are disposed in cleanup")]
    public class CategoryControllerTest : FinanceTestBase<CategoryControllerTestData>
    {
        #region Private Declarations

        private CategoryController categoryController;
        private HttpContextBase httpContextBase;
        private HttpSessionStateBase httpSessionStateBase;

        #endregion

        #region Initialization and cleanup

        /// <summary>
        /// Test initialization
        /// </summary>
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            categoryController = new CategoryController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            categoryController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                categoryController);
        }


        /// <summary>
        /// Test cleanup
        /// </summary>
        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();

            categoryController.GenericRepository = null;
            categoryController.Dispose();
            categoryController = null;
        }


        #endregion

        #region Logical Test

        /// <summary>
        /// Validates if the index action returns the expected result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 1)]
        public void Index_01_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            Category expectedCategory = TestData.UpsertCategory(applicationUserId: user.Id);

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index:1);
            TestData.UpsertCategory(applicationUserId: otherUser.Id, index:1);

            SessionHelper.TestUserId = user.Id;

            // Act
            ViewResult result = categoryController.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result is null");
            Assert.IsNotNull(result.Model, "Model is null");
            Assert.IsInstanceOf< List<Category>>(result.Model, "Model is incorrect type");

            List<Category> AvailableCategories = (List<Category>) result.Model;
            Assert.AreEqual(1, AvailableCategories.Count, "Invalid number of categories");
            Assert.AreEqual(expectedCategory.Id, AvailableCategories.First().Id, "Incorrect category returned");
        }

        /// <summary>
        /// Validates if the add action returns the expected result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 2)]
        public void AddCategory_02_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            Category availableParentCategory = TestData.UpsertCategory(applicationUserId: user.Id);
            SessionHelper.TestUserId = user.Id;

            ApplicationUser otherUser = TestData.UpsertApplicationUser(1);
            TestData.UpsertCategory(applicationUserId: otherUser.Id, index: 1);

            // Act
            ViewResult result = categoryController.AddCategory() as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result is null");
            Assert.IsNotNull(result.Model, "Model is null");
            Assert.IsInstanceOf< CategoryModel>(result.Model, "Model is incorrect type");

            CategoryModel categoryModel = (CategoryModel) result.Model;
            Assert.AreEqual(0, categoryModel.Id, "Model has incorrect id");

            Assert.AreEqual(2, categoryModel.AvailableCategories.Count(), "Invalid number of categories");
            Assert.IsNotNull(categoryModel.AvailableCategories, "Available categories is null");
            Assert.AreEqual("0", categoryModel.AvailableCategories.First().Value, "Available categories should include the none option");

            // First element is a null element for dropdown
            Assert.IsTrue(categoryModel.AvailableCategories.Any(item =>
                item.Value == availableParentCategory.Id.ToString(CultureInfo.CurrentCulture)), "Incorrect available category returned");
        }

        /// <summary>
        /// Validates if the RemoveCategory Action returns the expected result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 3)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Method is fine")]
        public void RemoveCategory_03_ValidCategoryId_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            Category category = TestData.UpsertCategory(applicationUserId: user.Id, index:0);
            Category expectedCategory = TestData.UpsertCategory(applicationUserId: user.Id, index: 1);

            SessionHelper.TestUserId = user.Id;

            // Act
            RedirectToRouteResult result = categoryController.RemoveCategory(category.Id) as RedirectToRouteResult;
            IList<Category> availableCategories = TestData.GenericRepository.FindAll<Category>(item => item.ApplicationUserId == user.Id);

            // Assert
            Assert.IsNotNull(result, "Result is null");
            Assert.AreEqual("Index", result.RouteValues["Action"], "Action was incorrectly set");
            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Success, Finances.Resources.CategoryRemoved);

            Assert.AreEqual(1, availableCategories.Count, "Incorrect number of categories available");
            Assert.AreEqual(expectedCategory.Id, availableCategories.First().Id);
        }

        /// <summary>
        /// Validates if the RemoveCategory Action fails correctly with an invalid category id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 4)]
        public void RemoveCategory_04_InvalidCategoryId_Fail()
        {
            // Arrange
            int categoryId = TestData.GenerateTestId(FinanceEntityType.Category);

            // Act
            RedirectToRouteResult result = categoryController.RemoveCategory(categoryId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result is null");
            Assert.AreEqual("Index", result.RouteValues["Action"], "Action was incorrectly set");
            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Error, Finances.Resources.CategoryNotFound);
        }


        /// <summary>
        /// Validates if the EditCategory Action returns the expected result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 5)]
        public void EditCategory_05_ValidCategoryId_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            Category availableParentCategory = TestData.UpsertCategory(applicationUserId:user.Id, index: 0);
            Category category = TestData.UpsertCategory(applicationUserId:user.Id, index: 1);
            CategoryMapping categoryMapping = TestData.UpsertCategoryMapping(category: category, columnType: ColumnType.Name, matchValue: string.Format(CultureInfo.CurrentCulture, "TestMatchValue_{0}", category.Id));

            SessionHelper.TestUserId = user.Id;

            // Act
            ViewResult result = categoryController.EditCategory(categoryId:category.Id) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< CategoryModel>(result.Model, "Model is incorrect type");

            CategoryModel categoryModel = (CategoryModel) result.Model;
            Assert.AreEqual(category.Name, categoryModel.Name, "Incorrect model returned");
            Assert.AreEqual(category.IsRegular, categoryModel.IsRegular, "Incorrect model returned");
            Assert.AreEqual(category.ColorCode, categoryModel.ColorCode, "Incorrect model returned");

            Assert.AreEqual(1, category.CategoryMappings.Count, "Incorrect number of mappings returned");
            Assert.IsTrue(category.CategoryMappings.Any(item => item.Id == categoryMapping.Id), "Category mapping was not included in model");
            
            Assert.IsFalse(categoryModel.AvailableCategories.Any(item =>
                item.Value == category.Id.ToString(CultureInfo.CurrentCulture)), "Available categories should not include current category");

            Assert.IsTrue(categoryModel.AvailableCategories.Any(item =>
                item.Value == availableParentCategory.Id.ToString(CultureInfo.CurrentCulture)), "Available categories should include upserted parent category");
            Assert.AreEqual("0", categoryModel.AvailableCategories.First().Value, "Available categories should include the none option");

            Assert.IsNull(categoryModel.AutomaticMappings, "Mappings should not be included");
        }


        /// <summary>
        /// Validates if the EditCategory Action fails correctly with an invalid category id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 6)]
        public void EditCategory_06_InvalidCategoryId_Fail()
        {
            // Arrange
            int categoryId = TestData.GenerateTestId(FinanceEntityType.Category);

            // Act
            RedirectToRouteResult result = categoryController.EditCategory(categoryId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");

            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Error, Finances.Resources.CategoryNotFound);
        }


        /// <summary>
        /// Validates if the editcategory returns the correct result when mappings are included
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 7)]
        public void EditCategory_07_IncludeMappings_Success()
        {
            // Arrange
            TestData.Test07_TestData();

            // Act
            ViewResult result = categoryController.EditCategory(categoryId: TestData.Test07_Category.Id, includeTransactions: true) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< CategoryModel>(result.Model, "Model is incorrect type");

            CategoryModel categoryModel = (CategoryModel)result.Model;
            Assert.AreEqual(TestData.Test07_Category.Id, categoryModel.Id, "Incorrect model returned");

            Assert.IsFalse(categoryModel.AvailableCategories.Any(item => item.Value == TestData.Test07_Category.Id.ToString(CultureInfo.CurrentCulture)), 
                "Available categories should not include current category");

            Assert.AreEqual("0", categoryModel.AvailableCategories.First().Value, "Available categories should have the none option first");

            Assert.AreEqual(1, categoryModel.AutomaticMappings.Count(), "Mappings should be included");
            Assert.AreEqual(TestData.Test07_ExpectedTransaction.Id, categoryModel.AutomaticMappings.First().Key.Id, "Mapping contains incorrecttransaction");
        }

        /// <summary>
        /// Validates if inserting a category inside the upsert excecutes correctly
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Test case is fine")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 8)]
        public void UpsertCategory_08_InsertCategory_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            Category parentCategory = TestData.UpsertCategory(applicationUserId: user.Id, index: 0);
            CategoryModel categoryModel = CategoryModel.CreateCategoryModel(TestData.CreateCategory(id: 0, index: 1, parentId: parentCategory.Id));
            SessionHelper.TestUserId = user.Id;

            // Act
            RedirectToRouteResult result = categoryController.UpsertCategory(categoryModel) as RedirectToRouteResult;
            Category upsertedCategory = categoryController.GenericRepository.FindSingle<Category>(item => item.ApplicationUserId == user.Id &&
                item.Name == categoryModel.Name);

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");

            Assert.AreEqual(categoryModel.ColorCode, upsertedCategory.ColorCode, "Color code was upserted incorrectly");
            Assert.AreEqual(categoryModel.IsRegular, upsertedCategory.IsRegular, "Isregular was upserted incorrectly");
            Assert.AreEqual(categoryModel.ParentId, upsertedCategory.ParentId, "Parent id was upsert incorrectly");

            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Success, Finances.Resources.CategoryCreated);
        }

        /// <summary>
        /// Validates if the update inside of the upsert excecutes correctly
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Method is fine")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 9)]
        public void UpsertCategory_09_UpdateCategory_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            Category parentCategory = TestData.UpsertCategory(applicationUserId:user.Id, index:0);
            Category category = TestData.UpsertCategory(applicationUserId:user.Id, index: 1, isRegular: true, colorCode: "#AAAAAA", parentId:parentCategory.Id);

            CategoryModel categoryModel = CategoryModel.CreateCategoryModel(TestData.CreateCategory(id: category.Id,
                name: string.Format(CultureInfo.CurrentCulture, "{0}_2", category.Name),
                colorCode: "#EEEEEE",
                isRegular: false,
                parentId: null));
            SessionHelper.TestUserId = user.Id;

            // Act
            RedirectToRouteResult result = categoryController.UpsertCategory(categoryModel) as RedirectToRouteResult;
            Category savedCategory = categoryController.GenericRepository.FindSingle<Category>(item => item.Id == category.Id);

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");

            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Success, Finances.Resources.CategoryUpdated);

            Assert.AreEqual(savedCategory.Name, categoryModel.Name);
            Assert.AreEqual(savedCategory.IsRegular, categoryModel.IsRegular);
            Assert.AreEqual(savedCategory.ColorCode, categoryModel.ColorCode);
            Assert.AreEqual(savedCategory.ParentId, categoryModel.ParentId);
        }

        /// <summary>
        /// Validates if an upsert with an invalid model fails correctly
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 10)]
        public void UpsertCategory_10_InvalidModel_Fail()
        {
            // Arrange
            CategoryModel categoryModel = CategoryModel.CreateCategoryModel(TestData.CreateCategory(id: 1));

            categoryController.ModelState.AddModelError("Message", "Ohnoes its an error");

            // Act
            ViewResult result = categoryController.UpsertCategory(categoryModel) as ViewResult;

            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "Model should not be null");
        }
        
        /// <summary>
        /// Validates if UpsertCategory excecutes correctly when the given category is not found
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 11)]
        public void UpsertCategory_11_UpdateCategoryNotFound_Fail()
        {
            // Arrange
            CategoryModel categoryModel = CategoryModel.CreateCategoryModel(TestData.CreateCategory(id: TestData.GenerateTestId(FinanceEntityType.Category)));

            // Act
            RedirectToRouteResult result = categoryController.UpsertCategory(categoryModel) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");

            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Error, Finances.Resources.CategoryNotFound);
        }

        /// <summary>
        /// Validates if AvailableParentCategories created the expected list
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 12)]
        public void AvailableParentCategories_12_ReturnsCorrectList_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            Category category = TestData.UpsertCategory(applicationUserId:user.Id, index: 0);
            Category expectedParentCategory = TestData.UpsertCategory(applicationUserId:user.Id, index: 1);

            SessionHelper.TestUserId = user.Id;
            
            // Act
            List<SelectListItem> result = categoryController.AvailableParentCategories(category).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(item => item.Value == expectedParentCategory.Id.ToString(CultureInfo.CurrentCulture)), "List does not contain expected category");
            Assert.IsFalse(result.Any(item => item.Value == category.Id.ToString(CultureInfo.CurrentCulture)), "List contains unexpected category");
            Assert.AreEqual(2, result.Count, "Invalid number of parent categories");

            SelectListItem firstItem = result.First();

            Assert.AreEqual(Finances.Resources.NoMainCategory, firstItem.Text, "First item has incorrect text");
            Assert.AreEqual("0", firstItem.Value, "First item has incorrect value");

            Assert.IsTrue(result.Any(item => item.Value == expectedParentCategory.Id.ToString(CultureInfo.CurrentCulture)), "Collection does not contain expected category");
        }

        /// <summary>
        /// Validates if an valid category id for the UpdateCategoryToMappedTransactions action returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 13)]
        public void UpdateCategoryToMappedTransactions_13_ValidCategory_Success()
        {
            TestData.Test14_TestData();

            // Act
            RedirectToRouteResult result = categoryController.UpdateCategoryToMappedTransactions(TestData.Test14_Category.Id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("EditCategory", result.RouteValues["action"], "Incorrect action");
            Assert.IsTrue(result.RouteValues.ContainsKey("categoryId"), "Redirect does not contain category id parameter");
            Assert.AreEqual(TestData.Test14_Category.Id, result.RouteValues["categoryId"], "Category id contains invalid value");

            IList<Transaction> mappedTransactions =
                categoryController.GenericRepository.FindAll<Transaction>(
                    item => item.TransactionCategories.Any(transactioncategory => transactioncategory.CategoryId == TestData.Test14_Category.Id)).ToList();

            Assert.AreEqual(1, mappedTransactions.Count, "Incorrect amount of mapped transactions mapped");
            Assert.AreEqual(TestData.Test14_ExpectedTransaction.Id, mappedTransactions.First().Id, "Incorrect transaction mapped");

            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Success, Finances.Resources.CategoryMappingsAppliedToTransactions);
        }

        /// <summary>
        /// Validates if an invalid category id for the UpdateCategoryToMappedTransactions action returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryController, 14)]
        public void UpdateCategoryToMappedTransactions_14_InvalidCategory_Fail()
        {
            // Arrange
            int invalidCategoryId = TestData.GenerateTestId(FinanceEntityType.Category);

            // Act
            RedirectToRouteResult result = categoryController.UpdateCategoryToMappedTransactions(invalidCategoryId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");

            ControllerTestHelper.AssertTemporaryMessage(categoryController.TempData, MessageState.Error, Finances.Resources.CategoryNotFound);
        }

        #endregion

        #region Contract Tests

        /// <summary>
        /// Validates if EditCategory throws the correct exception
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void EditCategory_ContractTest_InvalidCategoryId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                categoryController.EditCategory(-1);
            });
        }

        /// <summary>
        /// Validates if RemoveCategory throws the correct exception
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void RemoveCategory_ContractTest_InvalidCategoryId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                categoryController.RemoveCategory(-1);
            });
        }

        /// <summary>
        /// Validates if UpdateCategoryToMappedTransactions throws the correct exception
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void UpdateCategoryToMappedTransactions_ContractTest_InvalidCategoryId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                categoryController.UpdateCategoryToMappedTransactions(-1);
            });
        }

        /// <summary>
        /// Validates if UpsertCategory throws the correct exception
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void UpsertCategory_ContractTest_NullModel()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                categoryController.UpsertCategory(null);
            });
        }
        
        #endregion
    }
}

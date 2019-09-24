using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Finances.Bll.Classes;
using Finances.Bll.Handlers;
using Finances.Classes;
using Finances.Controllers;
using Finances.Domain.Entities;
using Finances.Models;
using Finances.UnitTestBase.Classes;
using Rhino.Mocks;
using NUnit.Framework;

namespace Finances.Web.Tests.Controllers
{
    /// <summary>
    /// Test class for the category mapping controller
    /// </summary>
    [TestFixture]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Objects are disposed in TestCleanup")]
    public class CategoryMappingControllerTest : FinanceTestBase<FinanceTestData>
    {
        #region Private members

        private CategoryMappingController categoryMappingController;
        private HttpContextBase httpContextBase;
        private HttpSessionStateBase httpSessionStateBase;

        #endregion

        #region Test Initialize / Cleanup

        /// <summary>
        /// Initializes the current test class
        /// </summary>
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            categoryMappingController = new CategoryMappingController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            categoryMappingController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                categoryMappingController);
        }

        /// <summary>
        /// Cleans up the current test class
        /// </summary>
        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();

            categoryMappingController.GenericRepository = null;
            categoryMappingController.Dispose();
            categoryMappingController = null;
        }

        #endregion

        #region Logical tests

        /// <summary>
        /// Validates if AddCategoryMapping returns the expected result with a valid id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 1)]
        public void AddCategoryMapping_01_ValidId_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            PartialViewResult result = categoryMappingController.AddCategoryMapping(category.Id) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("UpsertCategoryMapping", result.ViewName, "Incorrect viewname returned");
            Assert.IsInstanceOf<CategoryMappingModel>(result.Model, "Model is of incorrect type");

            CategoryMappingModel resultModel = (CategoryMappingModel)result.Model;
            Assert.AreEqual(category.Name, resultModel.CategoryName, "Incorrect name returned");
            Assert.AreEqual(category.Id, resultModel.CategoryId, "Incorrect category id inside of mapping");
        }

        /// <summary>
        /// Validates if AddCategoryMapping returns the expected result with an invalid id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 2)]
        public void AddCategoryMapping_02_InvalidId_Fail()
        {
            // Arrange
            int invalidCategoryId = TestData.GenerateTestId(FinanceEntityType.Category);

            // Act
            PartialViewResult result = categoryMappingController.AddCategoryMapping(invalidCategoryId) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNull(result.Model, "Model should be null");
            Assert.AreEqual("UpsertCategoryMapping", result.ViewName, "Incorrect viewname returned");
            ControllerTestHelper.AssertTemporaryMessage(categoryMappingController.TempData, MessageState.Error, Finances.Resources.CategoryNotFound);
        }

        /// <summary>
        /// Validates if EditCategoryMapping returns the expected result with a valid id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 3)]
        public void EditCategoryMapping_03_ValidId_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);
            CategoryMapping categoryMapping = TestData.UpsertCategoryMapping(category, ColumnType.Name);

            // Act
            PartialViewResult result = categoryMappingController.EditCategoryMapping(categoryMapping.Id) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("UpsertCategoryMapping", result.ViewName, "Incorrect viewname returned");
            Assert.IsInstanceOf<CategoryMappingModel>(result.Model, "Model is of incorrect type");

            CategoryMappingModel resultModel = (CategoryMappingModel)result.Model;
            Assert.AreEqual(category.Name, resultModel.CategoryName, "Incorrect name returned");
            Assert.AreEqual(category.Id, resultModel.CategoryId, "Incorrect category id");
            Assert.AreEqual(categoryMapping.Id, resultModel.Id, "Incorrect mapping id");
        }

        /// <summary>
        /// Validates if EditCategoryMapping returns the expected result with an invalid id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 4)]
        public void EditCategoryMapping_04_InvalidId_Fail()
        {
            // Arrange
            int invalidCategoryMappingId = TestData.GenerateTestId(FinanceEntityType.CategoryMapping);

            // Act
            PartialViewResult result = categoryMappingController.EditCategoryMapping(invalidCategoryMappingId) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNull(result.Model, "Model should be null");
            Assert.AreEqual("UpsertCategoryMapping", result.ViewName, "Incorrect viewname returned");
            ControllerTestHelper.AssertTemporaryMessage(categoryMappingController.TempData, MessageState.Error, Finances.Resources.CategoryMappingNotFound);
        }

        /// <summary>
        /// Validates if RemoveCategoryMapping returns the expected result with a valid id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 5)]
        public void RemoveCategoryMapping_05_ValidId_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);
            CategoryMapping categoryMapping = TestData.UpsertCategoryMapping(category, ColumnType.Name);

            // Act 
            RedirectToRouteResult result = categoryMappingController.RemoveCategoryMapping(categoryMapping.Id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("EditCategory", result.RouteValues["action"], "Invalid action returned");
            Assert.AreEqual("Category", result.RouteValues["controller"], "Invalid controller returned");
            Assert.AreEqual(category.Id, result.RouteValues["categoryId"], "Invalid category id in routevalues");

            ControllerTestHelper.AssertTemporaryMessage(categoryMappingController.TempData, MessageState.Success, Finances.Resources.CategoryMappingDeleted);
        }

        /// <summary>
        /// Validates if RemoveCategoryMapping returns the expected result with an invalid id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 6)]
        public void RemoveCategoryMapping_06_InvalidId_Fail()
        {
            // Arrange
            int invalidCategoryMappingId = TestData.GenerateTestId(FinanceEntityType.CategoryMapping);

            // Act
            RedirectToRouteResult result = categoryMappingController.RemoveCategoryMapping(invalidCategoryMappingId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("Index", result.RouteValues["action"], "Invalid action returned");
            Assert.AreEqual("Category", result.RouteValues["controller"], "Invalid controller returned");

            ControllerTestHelper.AssertTemporaryMessage(categoryMappingController.TempData, MessageState.Error, Finances.Resources.CategoryMappingNotFound);
        }

        /// <summary>
        /// Validates if UpsertCategoryMapping returns the expected result with an invalid column type
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 7)]
        public void UpsertCategoryMapping_07_InvalidColumnType_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);

            CategoryMappingModel model =
                CategoryMappingModel.CreateCategoryMappingModel(TestData.CreateCategoryMapping(id: TestData.GenerateTestId(FinanceEntityType.CategoryMapping),
                    category: category,
                    columnTypeId: ColumnType.AddSubtract));

            // Act
            PartialViewResult result = categoryMappingController.UpsertCategoryMapping(model) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.Model, "Model should not be null");
            Assert.IsInstanceOf<CategoryMappingModel>(result.Model, "Invalid model type");
            Assert.AreEqual("UpsertCategoryMapping", result.ViewName, "Incorrect viewname returned");

            Assert.IsTrue(categoryMappingController.ModelState.ContainsKey(""), "Modelstate does not contain expected key");
            Assert.AreEqual(1, categoryMappingController.ModelState[""].Errors.Count, "Invalid number of errors on model");
            Assert.AreEqual(Finances.Resources.UnsupportedColumnTypeMapping, categoryMappingController.ModelState[""].Errors.First().ErrorMessage, "Invalid error returned");
        }

        /// <summary>
        /// Validates if UpsertCategoryMapping returns the expected insert result with a valid mapping
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 8)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "All are needed")]
        public void UpsertCategoryMapping_08_InsertValidMapping_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            CategoryMappingModel model = CategoryMappingModel.CreateCategoryMappingModel(TestData.CreateCategoryMapping(id: 0,
                index: 1,
                category: category,
                columnTypeId: ColumnType.Name));

            // Act
            JsonResult result = categoryMappingController.UpsertCategoryMapping(model) as JsonResult;
            CategoryMapping upsertedCategoryMapping = categoryMappingController.GenericRepository.FindAll<CategoryMapping>(item => item.CategoryId == category.Id).SingleOrDefault();

            // Arrange
            Assert.IsNotNull(result, "Result should not be null");

            FinanceJsonResult jsonData = (FinanceJsonResult)result.Data;
            Assert.AreEqual(jsonData.Success, true, "Invalid success paramter set");
            Assert.AreEqual(jsonData.ErrorMessage, null, "Invalid error message set");

            Assert.IsNotNull(upsertedCategoryMapping, "Category mapping was not upserted correctly");
            Assert.AreEqual(ColumnType.Name, upsertedCategoryMapping.ColumnTypeId, "ColumnType was not upserted properly");
            Assert.AreEqual(model.MatchValue, upsertedCategoryMapping.MatchValue, "MatchValue was not upserted properly");
        }

        /// <summary>
        /// Validates if UpsertCategoryMapping returns the expected update result with a valid mapping
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 9)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "All are needed")]
        public void UpsertCategoryMapping_09_UpdateValidMapping_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);
            CategoryMapping categoryMapping = TestData.UpsertCategoryMapping(category: category, columnType: ColumnType.Name);

            CategoryMappingModel model = CategoryMappingModel.CreateCategoryMappingModel(TestData.CreateCategoryMapping(id: categoryMapping.Id,
                category: category,
                columnTypeId: ColumnType.Description,
                matchValue: categoryMapping.MatchValue + "_2"));

            // Act
            JsonResult result = categoryMappingController.UpsertCategoryMapping(model) as JsonResult;
            CategoryMapping upsertedCategoryMapping = categoryMappingController.GenericRepository.FindAll<CategoryMapping>(item => item.CategoryId == category.Id).SingleOrDefault();

            // Arrange
            Assert.IsNotNull(result, "Result should not be null");

            FinanceJsonResult jsonData = (FinanceJsonResult)result.Data;
            Assert.AreEqual(jsonData.Success, true, "Invalid success paramter set");
            Assert.AreEqual(jsonData.ErrorMessage, null, "Invalid error message set");

            Assert.IsNotNull(upsertedCategoryMapping, "Category mapping was not upserted correctly");
            Assert.AreEqual(ColumnType.Description, upsertedCategoryMapping.ColumnTypeId, "ColumnType was not upserted properly");
            Assert.AreEqual(model.MatchValue, upsertedCategoryMapping.MatchValue, "MatchValue was not upserted properly");
        }

        /// <summary>
        /// Validates if UpsertCategoryMapping returns the expected result with an invalid update of a non-existing mapping
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 10)]
        public void UpsertCategoryMapping_10_UpdateNonExistingMapping_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            CategoryMappingModel model =
                CategoryMappingModel.CreateCategoryMappingModel(TestData.CreateCategoryMapping(id: TestData.GenerateTestId(FinanceEntityType.CategoryMapping),
                    category: category,
                    columnTypeId: ColumnType.Name));

            // Act
            PartialViewResult result = categoryMappingController.UpsertCategoryMapping(model) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.Model, "Model should not be null");
            Assert.IsInstanceOf<CategoryMappingModel>(result.Model, "Invalid model type");
            Assert.AreEqual("UpsertCategoryMapping", result.ViewName, "Incorrect viewname returned");

            ControllerTestHelper.AssertTemporaryMessage(categoryMappingController.TempData, MessageState.Error, Finances.Resources.CategoryMappingNotFound);
        }

        /// <summary>
        /// Validates if UpsertCategoryMapping returns the expected insert result with an invalid model state
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.CategoryMappingController, 11)]
        public void UpsertCategoryMapping_11_InvalidModelState_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);
            CategoryMappingModel model =
                CategoryMappingModel.CreateCategoryMappingModel(TestData.CreateCategoryMapping(id: TestData.GenerateTestId(FinanceEntityType.CategoryMapping),
                    category: category,
                    columnTypeId: ColumnType.Name));

            categoryMappingController.ModelState.AddModelError("Error", "The modelstate is now invalid");

            // Act
            PartialViewResult result = categoryMappingController.UpsertCategoryMapping(model) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.Model, "Model should not be null");
            Assert.IsInstanceOf<CategoryMappingModel>(result.Model, "Invalid model type");
            Assert.AreEqual("UpsertCategoryMapping", result.ViewName, "Incorrect viewname returned");
        }

        #endregion

        #region Contract Tests

        /// <summary>
        /// Contract test for the AddCategoryMapping method with an invalid category id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void AddCategoryMapping_ContractTest_InvalidCategoryId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                categoryMappingController.AddCategoryMapping(-1);
            });
        }

        /// <summary>
        /// Contract test for the EditCategoryMapping method with an invalid category id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void EditCategoryMapping_ContractTest_InvalidMappingId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                categoryMappingController.EditCategoryMapping(-1);
            });
        }

        /// <summary>
        /// Contract test for the UpsertCategoryMapping method with an invalid category id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void UpsertCategoryMapping_ContractTest_NullModel()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                categoryMappingController.UpsertCategoryMapping(null);
            });
        }

        /// <summary>
        /// Contract test for the RemoveCategoryMapping method with an invalid category id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void RemoveCategoryMapping_ContractTest_NullModel()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                categoryMappingController.RemoveCategoryMapping(-1);
            });
        }


        #endregion
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Finances.Bll.Classes;
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
    /// Test class for the import controller
    /// </summary>
    [TestFixture]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Nope")]
    public class ImportControllerTest : FinanceTestBase<ImportControllerTestData>
    {
        #region Private Declarations

        private ImportController importController;
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

            importController = new ImportController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            importController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                importController);
        }

        /// <summary>
        /// Test cleanup
        /// </summary>
        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();

            importController.GenericRepository = null;
            importController.Dispose();
            importController = null;
        }

        #endregion

        #region Logical tests

        /// <summary>
        /// Validates if the index action returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 1)]
        public void Index_01_Success()
        {
            // Arrange
            ImportBank importBank = TestData.UpsertImportBank();

            // Act
            ActionResult result = importController.Index();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Incorrect type returned");

            ViewResult viewResult = (ViewResult) result;
            Assert.AreEqual("Index", viewResult.ViewName, "Incorrect view name");
            Assert.IsInstanceOf< ImportModel>(viewResult.Model, "Model is of incorrect type");

            ImportModel importModel = (ImportModel) viewResult.Model;
            Assert.IsNotNull(importModel.AvailableImportBanks, "Available import banks should not be null");
            Assert.IsTrue(importModel.AvailableImportBanks.Any(item => item.Id == importBank.Id && item.Name == importBank.Name), 
                "Incorrect import bank returned");
        }

        /// <summary>
        /// Validates if StartImport returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 2)]
        public void StartImport_02_Success()
        {
            // Arrange
            ImportModel importModel = new ImportModel();

            // Act
            ActionResult result = importController.StartImport(importModel);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Incorrect type returned");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("StartImport", viewResult.ViewName, "Incorrect view name");
            Assert.IsInstanceOf< ImportModel>(viewResult.Model, "Model is of incorrect type");
            Assert.AreEqual(importModel, viewResult.Model, "Incorrect model returned");
        }

        /// <summary>
        /// Validates if StartImport returns the correct result
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Needed")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 3)]
        public void Import_03_ValidIngImport_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            TestData.Upsert_Ing_ImportConfiguration();
            TestData.Setup_Valid_Ing_Import();

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ImportModel importModel = new ImportModel
            {
                ImportBankId = TestData.IngImportBank.Id,
                BankAccountId = TestData.DefaultCheckingBankAccount.Id
            };

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(TestData.IngImportFileContents);
                writer.Flush();
                stream.Position = 0;

                HttpPostedFileBase file = MockRepository.GenerateStub<HttpPostedFileBase>();
                file.Stub(item => item.InputStream).Return(stream);

                // Act
                ActionResult result = importController.Import(file, importModel);

                // Assert
                Assert.IsNotNull(result, "Result should not be null");
                Assert.IsInstanceOf< ViewResult>(result, "Incorrect type returned");

                ViewResult viewResult = (ViewResult) result;
                Assert.AreEqual("ImportResult", viewResult.ViewName, "Incorrect view name");
                Assert.IsInstanceOf< ImportModel>(viewResult.Model, "Model is of incorrect type");

                ImportModel actualImportModel = (ImportModel) viewResult.Model;
                Assert.IsNotNull(actualImportModel.ImportRows, "Import rows should not be null");
                Assert.AreEqual(2, actualImportModel.ImportRows.Count, "Incorrect number of rows returned");

                ImportRow firstImportRow = actualImportModel.ImportRows.ElementAt(0);
                ImportRow secondImportRow = actualImportModel.ImportRows.ElementAt(1);

                Assert.AreEqual(0, firstImportRow.ImportRowId, "Invalid first row import id");
                Assert.AreEqual(1, secondImportRow.ImportRowId, "Invalid second row import id");
                Assert.AreEqual(false, firstImportRow.ExistsInDatabase, "First import row should not exist in database");
                Assert.AreEqual(false, secondImportRow.ExistsInDatabase, "Second import row should not exist in database");
                Assert.AreEqual(true, firstImportRow.Import, "First import row should default set to true for import");
                Assert.AreEqual(true, secondImportRow.Import, "Second import row should default set to true for import");

                Compare_Transactions(TestData.IngExpectedTransaction01, firstImportRow.Transaction);
                Compare_Transactions(TestData.IngExpectedTransaction02, secondImportRow.Transaction);
            }
        }

        /// <summary>
        /// Validates if StartImport returns the correct result when an invalid file is inserted
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 4)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Needed")]
        public void Import_04_InvalidImportFile_Fail()
        {
            TestData.UpsertDefaultUserBankAccounts();
            TestData.Upsert_Ing_ImportConfiguration();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ImportModel importModel = new ImportModel
            {
                ImportBankId = TestData.IngImportBank.Id,
                BankAccountId = TestData.DefaultCheckingBankAccount.Id
            };

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(Resources.PictureImport);
                writer.Flush();
                stream.Position = 0;

                HttpPostedFileBase file = MockRepository.GenerateStub<HttpPostedFileBase>();
                file.Stub(item => item.InputStream).Return(stream);

                // Act
                ActionResult result = importController.Import(file, importModel);

                // Assert
                Assert.IsNotNull(result, "Result should not be null");
                Assert.IsInstanceOf< ViewResult>(result, "Incorrect type returned");

                ViewResult viewResult = (ViewResult)result;
                Assert.AreEqual("ImportResult", viewResult.ViewName, "Incorrect view name");
                Assert.IsInstanceOf< ImportModel>(viewResult.Model, "Model is of incorrect type");

                ImportModel actualImportModel = (ImportModel)viewResult.Model;
                Assert.IsNotNull(actualImportModel.ImportRows, "Import rows should not be null");
                Assert.AreEqual(0, actualImportModel.ImportRows.Count, "Incorrect number of rows returned");
            }
        }

        /// <summary>
        /// Validates if SaveImport returns the correct result when no cache entry is present
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 5)]
        public void SaveImport_05_NoCacheEntry_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            TestData.Upsert_Ing_ImportConfiguration();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ImportModel importModel = new ImportModel
            {
                ImportBankId = TestData.IngImportBank.Id
            };

            // Act
            ActionResult result = importController.SaveImport(importModel);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Incorrect type returned");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("Index", viewResult.ViewName, "Incorrect view name");
            Assert.IsNull(viewResult.Model, "Model should be null");

            ControllerTestHelper.AssertTemporaryMessage(importController.TempData, MessageState.Warning, Finances.Resources.ImportTimeOut);
        }

        /// <summary>
        /// Validates if SaveImport returns the correct result when there is a valid cache entry available
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 6)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void SaveImport_06_ValidCacheEntry_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            TestData.Upsert_Ing_ImportConfiguration();
            TestData.Setup_Valid_Ing_Import();

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // User and bank should be set after saving
            TestData.IngExpectedTransaction01.ApplicationUserId = TestData.DefaultTestApplicationUser.Id;
            TestData.IngExpectedTransaction02.ApplicationUserId = TestData.DefaultTestApplicationUser.Id;

            TestData.IngExpectedTransaction01.BankAccountId = TestData.DefaultCheckingBankAccount.Id;
            TestData.IngExpectedTransaction02.BankAccountId = TestData.DefaultCheckingBankAccount.Id;

            ImportModel importModel = new ImportModel
            {
                ImportBankId = TestData.IngImportBank.Id,
                BankAccountId = TestData.DefaultCheckingBankAccount.Id
            };

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(TestData.IngImportFileContents);
                writer.Flush();
                stream.Position = 0;

                HttpPostedFileBase file = MockRepository.GenerateStub<HttpPostedFileBase>();
                file.Stub(item => item.InputStream).Return(stream);

                // Act
                ViewResult importResult = (ViewResult) importController.Import(file, importModel);
                ActionResult saveImportResult = importController.SaveImport(importResult.Model as ImportModel);
                
                // Assert
                Transaction upsertedTransaction01 = importController.GenericRepository.FindSingle<Transaction>(item => item.Name == TestData.IngExpectedTransaction01.Name);
                Transaction upsertedTransaction02 = importController.GenericRepository.FindSingle<Transaction>(item => item.Name == TestData.IngExpectedTransaction02.Name);

                Assert.IsNotNull(saveImportResult, "Result should not be null");
                Assert.IsInstanceOf< RedirectToRouteResult>(saveImportResult, "Incorrect type returned");

                RedirectToRouteResult redirectResult = (RedirectToRouteResult) saveImportResult;
                Assert.AreEqual("AccountOverview", redirectResult.RouteValues["controller"], "Incorrect controller name returned");
                Assert.AreEqual("Index", redirectResult.RouteValues["action"], "Incorrect controller name returned");

                Assert.IsNotNull(upsertedTransaction01, "First transaction was not saved");
                Assert.IsNotNull(upsertedTransaction02, "Second transaction was not saved");
                Compare_Transactions(transactionExpected: upsertedTransaction01, transactionActual: TestData.IngExpectedTransaction01, includeIds: false);
                Compare_Transactions(transactionExpected: upsertedTransaction02, transactionActual: TestData.IngExpectedTransaction02, includeIds: false);
                ControllerTestHelper.AssertTemporaryMessage(importController.TempData, MessageState.Success,
                    string.Format(CultureInfo.CurrentCulture, Finances.Resources.TransactionsAddedAndSkippedFormat, 2, 0));
            }
        }

        /// <summary>
        /// Validates if importing with an invalid Importbank id fails correctly
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 7)]
        public void Import_07_InvalidImportBank_Fail()
        {
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ImportModel importModel = new ImportModel
            {
                ImportBankId = TestData.GenerateTestId(FinanceEntityType.ImportBank)
            };

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(Resources.INGImport);
                writer.Flush();
                stream.Position = 0;

                HttpPostedFileBase file = MockRepository.GenerateStub<HttpPostedFileBase>();
                file.Stub(item => item.InputStream).Return(stream);

                // Act
                ActionResult result = importController.Import(file, importModel);

                // Assert
                Assert.IsNotNull(result, "Result should not be null");
                Assert.IsInstanceOf< RedirectToRouteResult>(result, "Incorrect type returned");

                RedirectToRouteResult redirectResult = (RedirectToRouteResult)result;
                Assert.AreEqual("Index", redirectResult.RouteValues["action"], "invalid action returned");

                ControllerTestHelper.AssertTemporaryMessage(importController.TempData, MessageState.Error, Finances.Resources.Error);
            }
        }

        /// <summary>
        /// Validates if import returns the correct result when a transaction already exists
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 8)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void Import_08_ExistingTransaction_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            TestData.Upsert_Ing_ImportConfiguration();
            TestData.Setup_Valid_Ing_Import();

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ImportModel importModel = new ImportModel
            {
                ImportBankId = TestData.IngImportBank.Id,
                BankAccountId = TestData.DefaultCheckingBankAccount.Id
            };

            // Upsert the first expected transaction to the database
            TestData.IngExpectedTransaction01.ApplicationUserId = TestData.DefaultTestApplicationUser.Id;
            TestData.IngExpectedTransaction01.BankAccountId = TestData.DefaultCheckingBankAccount.Id;

            importController.GenericRepository.Insert(TestData.IngExpectedTransaction01);
            importController.GenericRepository.Save();

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(TestData.IngImportFileContents);
                writer.Flush();
                stream.Position = 0;

                HttpPostedFileBase file = MockRepository.GenerateStub<HttpPostedFileBase>();
                file.Stub(item => item.InputStream).Return(stream);

                // Act
                ActionResult result = importController.Import(file, importModel);

                // Assert
                Assert.IsNotNull(result, "Result should not be null");
                Assert.IsInstanceOf< ViewResult>(result, "Incorrect type returned");

                ViewResult viewResult = (ViewResult)result;
                Assert.AreEqual("ImportResult", viewResult.ViewName, "Incorrect view name");
                Assert.IsInstanceOf< ImportModel>(viewResult.Model, "Model is of incorrect type");

                ImportModel actualImportModel = (ImportModel)viewResult.Model;
                Assert.IsNotNull(actualImportModel.ImportRows, "Import rows should not be null");
                Assert.AreEqual(2, actualImportModel.ImportRows.Count, "Incorrect number of rows returned");

                ImportRow firstImportRow = actualImportModel.ImportRows.ElementAt(0);
                ImportRow secondImportRow = actualImportModel.ImportRows.ElementAt(1);

                Assert.AreEqual(0, firstImportRow.ImportRowId, "Invalid first row import id");
                Assert.AreEqual(1, secondImportRow.ImportRowId, "Invalid second row import id");
                Assert.AreEqual(true, firstImportRow.ExistsInDatabase, "First import row should not exist in database");
                Assert.AreEqual(false, secondImportRow.ExistsInDatabase, "Second import row should not exist in database");
                Assert.AreEqual(false, firstImportRow.Import, "First import row should default set to true for import");
                Assert.AreEqual(true, secondImportRow.Import, "Second import row should default set to true for import");

                Compare_Transactions(TestData.IngExpectedTransaction01, firstImportRow.Transaction, includeIds: false, includeBankAndUser: false);
                Compare_Transactions(TestData.IngExpectedTransaction02, secondImportRow.Transaction);
            }
        }

        /// <summary>
        /// Validates if the import is set to false for an import row the row does not get saved to the database
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ImportController, 9)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void SaveImport_09_ImportFalse_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            TestData.Upsert_Ing_ImportConfiguration();
            TestData.Setup_Valid_Ing_Import();

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // User and bank should be set after saving
            TestData.IngExpectedTransaction01.ApplicationUserId = TestData.DefaultTestApplicationUser.Id;
            TestData.IngExpectedTransaction02.ApplicationUserId = TestData.DefaultTestApplicationUser.Id;

            TestData.IngExpectedTransaction01.BankAccountId = TestData.DefaultCheckingBankAccount.Id;
            TestData.IngExpectedTransaction02.BankAccountId = TestData.DefaultCheckingBankAccount.Id;

            ImportModel importModel = new ImportModel
            {
                ImportBankId = TestData.IngImportBank.Id,
                BankAccountId = TestData.DefaultCheckingBankAccount.Id
            };

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(TestData.IngImportFileContents);
                writer.Flush();
                stream.Position = 0;

                HttpPostedFileBase file = MockRepository.GenerateStub<HttpPostedFileBase>();
                file.Stub(item => item.InputStream).Return(stream);

                ViewResult importResult = (ViewResult)importController.Import(file, importModel);

                // Set the first import row to not import
                ImportModel importResultModel = (ImportModel) importResult.Model;
                importResultModel.ImportRows.First().Import = false;

                // Act
                ActionResult saveImportResult = importController.SaveImport(importResultModel);

                // Assert
                Transaction upsertedTransaction01 = importController.GenericRepository.FindSingle<Transaction>(item => item.Name == TestData.IngExpectedTransaction01.Name);
                Transaction upsertedTransaction02 = importController.GenericRepository.FindSingle<Transaction>(item => item.Name == TestData.IngExpectedTransaction02.Name);

                Assert.IsNotNull(saveImportResult, "Result should not be null");
                Assert.IsInstanceOf< RedirectToRouteResult>(saveImportResult, "Incorrect type returned");

                RedirectToRouteResult redirectResult = (RedirectToRouteResult)saveImportResult;
                Assert.AreEqual("AccountOverview", redirectResult.RouteValues["controller"], "Incorrect controller name returned");
                Assert.AreEqual("Index", redirectResult.RouteValues["action"], "Incorrect controller name returned");

                Assert.IsNull(upsertedTransaction01, "First transaction should not be saved");
                Assert.IsNotNull(upsertedTransaction02, "Second transaction was not saved");
                Compare_Transactions(transactionExpected: upsertedTransaction02, transactionActual: TestData.IngExpectedTransaction02, includeIds: false);
                ControllerTestHelper.AssertTemporaryMessage(importController.TempData, MessageState.Success,
                    string.Format(CultureInfo.CurrentCulture, Finances.Resources.TransactionsAddedAndSkippedFormat, 1, 1));
            }
        }

        #endregion

        #region Contract tests

        /// <summary>
        /// Validates if StartImport returns the expected exception for a null file
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void Import_NullFile_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                importController.Import(file: null, model: null);
            });
        }

        /// <summary>
        /// Validates if Import returns the expected exception for a null model
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void Import_NullModel_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(Resources.INGImport);
                writer.Flush();
                stream.Position = 0;

                HttpPostedFileBase file = MockRepository.GenerateStub<HttpPostedFileBase>();
                file.Stub(item => item.InputStream).Return(stream);

                importController.Import(file: file, model: null);
                }
            });
        }

        /// <summary>
        /// Validates if SaveImport returns the expected exception for a null model
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void SaveImport_NullModel_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                importController.SaveImport(model: null);
            });
        }

        /// <summary>
        /// Validates if StartImport returns the expected exception for a null model
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void StartImport_NullModel_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                importController.StartImport(model: null);
            });
        }

        #endregion
    }
}

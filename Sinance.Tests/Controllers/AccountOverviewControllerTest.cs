using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    /// Tests for the account overview controller
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Objects are disposed in cleanup")]
    [TestFixture]
    public class AccountOverviewControllerTest : FinanceTestBase<FinanceTestData>
    {
        #region Private Declarations

        private AccountOverviewController accountOverviewController;
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

            accountOverviewController = new AccountOverviewController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            accountOverviewController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                accountOverviewController);
        }

        /// <summary>
        /// Test cleanup
        /// </summary>
        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();

            accountOverviewController.GenericRepository = null;
            accountOverviewController.Dispose();
            accountOverviewController = null;
        }

        #endregion

        #region Logical tests

        /// <summary>
        /// Validates if requesting the overview with a valid bank account returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 1)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Method is fine")]
        public void Index_01_ValidBankAccountId_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Transaction transaction = TestData.UpsertTransaction(applicationUserId: TestData.DefaultTestApplicationUser.Id,
                bankAccountId: TestData.DefaultCheckingBankAccount.Id, amount: 100);
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;
            IList<Category> expectedCategories = TestData.GenericRepository.FindAll<Category>(item => item.ApplicationUserId == TestData.DefaultTestApplicationUser.Id);

            decimal expectedAccountBalance = TestData.DefaultCheckingBankAccount.StartBalance + transaction.Amount;

            // Act
            ActionResult result = accountOverviewController.Index(TestData.DefaultCheckingBankAccount.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("index", viewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(viewResult.Model, "Model should be null");
            Assert.IsInstanceOf<AccountOverviewModel>(viewResult.Model, "Result model is of incorrect type");

            AccountOverviewModel resultModel = (AccountOverviewModel)viewResult.Model;
            Assert.AreEqual(expectedAccountBalance, resultModel.AccountBalance, "Incorrect account balance returned");
            Assert.AreEqual(1, resultModel.Transactions.Count(), "Invalid number of transactions returned");
            Assert.AreEqual(transaction.Id, resultModel.Transactions.First().Id, "Invalid transaction returned");
            Assert.AreEqual(expectedCategories.Count(), resultModel.AvailableCategories.Count(), "Invalid number of available categories returned");
        }

        /// <summary>
        /// Validates if requesting the overview for an invalid bank account results in the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 2)]
        public void Index_02_InvalidBankAccountId_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            int invalidBankAccountId = TestData.GenerateTestId(FinanceEntityType.BankAccount, 10);

            // Act
            ActionResult result = accountOverviewController.Index(invalidBankAccountId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("index", viewResult.ViewName, "Incorrect view name");
            Assert.IsNull(viewResult.Model, "Model should be null");
            ControllerTestHelper.AssertTemporaryMessage(accountOverviewController.TempData, MessageState.Error, Finances.Resources.BankAccountNotFound);
        }

        /// <summary>
        /// Validates if trying to access a bank account of another fails correctly
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 3)]
        public void Index_03_NoAccessToBankAccount_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();

            ApplicationUser user = TestData.UpsertApplicationUser(1);
            BankAccount bankAccount = TestData.UpsertBankAccount(userId: user.Id, index: 1);

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = accountOverviewController.Index(bankAccount.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("index", viewResult.ViewName, "Incorrect view name");
            Assert.IsNull(viewResult.Model, "Model should be null");
            ControllerTestHelper.AssertTemporaryMessage(accountOverviewController.TempData, MessageState.Error, Finances.Resources.BankAccountNotFound);
        }

        /// <summary>
        /// Validates if starting the add transaction with a valid bank returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 4)]
        public void AddTransaction_04_ValidBankAccount_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = accountOverviewController.AddTransaction(TestData.DefaultCheckingBankAccount.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult viewResult = (PartialViewResult)result;
            Assert.AreEqual("UpsertTransactionPartial", viewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOf<TransactionModel>(viewResult.Model, "Incorrect model type returned");

            TransactionModel transactionModel = (TransactionModel)viewResult.Model;
            Assert.AreEqual(TestData.DefaultCheckingBankAccount.Id, transactionModel.BankAccountId, "Incorrect bank account id set");
            Assert.AreEqual(DateTime.Now.Date, transactionModel.Date.Date, "Incorrect date set");
        }

        /// <summary>
        /// Validates if starting the add transaction with a invalid bank with no access returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 5)]
        public void AddTransaction_05_InvalidBankAccountNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ApplicationUser user = TestData.UpsertApplicationUser(index: 1);
            BankAccount bankAccount = TestData.UpsertBankAccount(user.Id, index: 1);

            // Act
            ActionResult result = accountOverviewController.AddTransaction(bankAccount.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult viewResult = (PartialViewResult)result;
            Assert.AreEqual("UpsertTransactionPartial", viewResult.ViewName, "Incorrect view name");
            Assert.IsNull(viewResult.Model, "Model should be null");

            ControllerTestHelper.AssertTemporaryMessage(accountOverviewController.TempData, MessageState.Error, Finances.Resources.BankAccountNotFound);
        }

        /// <summary>
        /// Validates if edit transaction returns the correct result with a valid transaction id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 6)]
        public void EditTransaction_06_ValidTransactionId_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Transaction transaction = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id);
            Category category = TestData.UpsertCategory(applicationUserId: TestData.DefaultTestApplicationUser.Id);

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = accountOverviewController.EditTransaction(transaction.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult viewResult = (PartialViewResult)result;
            Assert.AreEqual("UpsertTransactionPartial", viewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOf<TransactionModel>(viewResult.Model, "Incorrect model type returned");

            TransactionModel transactionModel = (TransactionModel)viewResult.Model;
            Assert.AreEqual(transaction.Id, transactionModel.Id, "Id are not equal");
            Assert.AreEqual(transaction.Name, transactionModel.Name, "name are not equal");
            Assert.AreEqual(transaction.Description, transactionModel.Description, "Description are not equal");
            Assert.AreEqual(transaction.DestinationAccount, transactionModel.DestinationAccount, "Destination account are not equal");
            Assert.AreEqual(transaction.BankAccountId, transactionModel.BankAccountId, "Bank account id are not equal");
            Assert.AreEqual(transaction.Amount, transactionModel.Amount, "Amount are not equal");
            Assert.IsTrue(transaction.TransactionCategories.SequenceEqual(transactionModel.TransactionCategories), "TransactionCategories are not equal");

            Assert.IsNotNull(transactionModel.AvailableCategories, "Available categories should not be null");
            Assert.AreEqual("0", transactionModel.AvailableCategories.First().Value, "Incorrect value for first available category");
            Assert.AreEqual(category.Id.ToString(CultureInfo.CurrentCulture), transactionModel.AvailableCategories.ElementAt(1).Value, "Incorrect value for second available category");
        }

        /// <summary>
        /// Validates if edit transaction returns the correct result with a invalid transaction id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 7)]
        public void EditTransaction_07_InvalidTransactionIdNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index: 1);
            BankAccount otherBankAccount = TestData.UpsertBankAccount(userId: otherUser.Id, index: 1);
            Transaction otherTransaction = TestData.UpsertTransaction(bankAccountId: otherBankAccount.Id, applicationUserId: otherUser.Id);

            // Act
            ActionResult result = accountOverviewController.EditTransaction(otherTransaction.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult viewResult = (PartialViewResult)result;
            Assert.AreEqual("UpsertTransactionPartial", viewResult.ViewName, "Incorrect view name");
            Assert.IsNull(viewResult.Model, "Model should be null");
            ControllerTestHelper.AssertTemporaryMessage(accountOverviewController.TempData, MessageState.Error, Finances.Resources.TransactionNotFound);
        }

        /// <summary>
        /// Validates if deleting a transaction returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 8)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void DeleteTransaction_08_ValidTransactionId_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            Transaction transaction = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, amount: 100);

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = accountOverviewController.DeleteTransaction(transaction.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<RedirectToRouteResult>(result, "Result is of incorrect type");

            RedirectToRouteResult viewResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", viewResult.RouteValues["action"], "Incorrect view name returned");

            Transaction deletedTransaction = TestData.GenericRepository.FindSingle<Transaction>(item => item.Id == transaction.Id);
            Assert.IsNull(deletedTransaction, "Transaction was not deleted from database");

            Assert.AreEqual(SessionHelper.BankAccounts.First().CurrentBalance, TestData.DefaultCheckingBankAccount.StartBalance, "Balance was not updated on bank account");

            ControllerTestHelper.AssertTemporaryMessage(accountOverviewController.TempData, MessageState.Success, Finances.Resources.TransactionDeleted);
        }

        /// <summary>
        /// Validates if deleting a transaction with an incorrect id fails correctly
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 9)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void DeleteTransaction_09_InvalidTransactionIdNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index: 1);
            BankAccount otherBankAccount = TestData.UpsertBankAccount(userId: otherUser.Id, index: 1);
            Transaction otherTransaction = TestData.UpsertTransaction(bankAccountId: otherBankAccount.Id, applicationUserId: otherUser.Id);

            // Act
            ActionResult result = accountOverviewController.DeleteTransaction(otherTransaction.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<RedirectToRouteResult>(result, "Result is of incorrect type");

            RedirectToRouteResult viewResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", viewResult.RouteValues["action"], "Incorrect view name returned");

            ControllerTestHelper.AssertTemporaryMessage(accountOverviewController.TempData, MessageState.Error, Finances.Resources.TransactionNotFound);

            Transaction transaction = TestData.GenericRepository.FindSingle<Transaction>(item => item.Id == otherTransaction.Id);
            Assert.IsNotNull(transaction, "Transaction was incorrectly deleted");
        }

        /// <summary>
        /// Validates if upserting a transaction with a new transaction returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 10)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void UpsertTransaction_10_NewTransaction_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            Transaction transaction = FinanceTestData.CreateTransaction(id: TestData.GenerateTestId(FinanceEntityType.Transaction),
                bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id);
            TransactionModel transactionModel = TransactionModel.CreateTransactionModel(transaction);
            transactionModel.Id = 0;

            // Act
            ActionResult result = accountOverviewController.UpsertTransaction(transactionModel);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;
            Assert.AreEqual(jsonData.Success, true, "Invalid success paramter set");
            Assert.AreEqual(jsonData.ErrorMessage, null, "Invalid error message set");

            Transaction insertedTransaction = TestData.GenericRepository.FindSingle<Transaction>(item => item.Name == transaction.Name);
            Assert.IsNotNull(insertedTransaction, "Transaction was not inserted correctly");

            Assert.AreEqual(transaction.Date, insertedTransaction.Date, "Date is not equal");
            Assert.AreEqual(transaction.Description, insertedTransaction.Description, "Description are not equal");
            Assert.AreEqual(transaction.DestinationAccount, insertedTransaction.DestinationAccount, "Destination account are not equal");
            Assert.AreEqual(transaction.BankAccountId, insertedTransaction.BankAccountId, "Bank account id are not equal");
            Assert.AreEqual(transaction.Amount, insertedTransaction.Amount, "Amount are not equal");

            decimal expectedBankBalance = TestData.DefaultCheckingBankAccount.StartBalance + transaction.Amount;
            Assert.AreEqual(expectedBankBalance, SessionHelper.BankAccounts.Sum(item => item.CurrentBalance), "Incorrect current balance");
        }

        /// <summary>
        /// Validates if updating a existing transaction returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 11)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void UpsertTransaction_11_UpdateTransaction_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            Transaction transaction = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id);
            TransactionModel transactionModel = new TransactionModel
            {
                Id = transaction.Id,
                BankAccountId = transaction.BankAccountId,
                Amount = transaction.Amount + 100,
                Name = string.Format(CultureInfo.CurrentUICulture, "{0}_2", transaction.Name),
                Date = transaction.Date.AddDays(-1),
                Description = string.Format(CultureInfo.CurrentUICulture, "{0}_2", transaction.Description),
                DestinationAccount = string.Format(CultureInfo.CurrentUICulture, "{0}_2", transaction.DestinationAccount)
            };

            // Act
            ActionResult result = accountOverviewController.UpsertTransaction(transactionModel);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;
            Assert.AreEqual(jsonData.Success, true, "Invalid success paramter set");
            Assert.AreEqual(jsonData.ErrorMessage, null, "Invalid error message set");

            Transaction updatedTransaction = accountOverviewController.GenericRepository.FindSingle<Transaction>(item => item.Name == transactionModel.Name);
            Assert.IsNotNull(updatedTransaction, "Transaction was not updated correctly");

            Assert.AreEqual(transactionModel.Date, updatedTransaction.Date, "Date is not equal");
            Assert.AreEqual(transactionModel.Description, updatedTransaction.Description, "Description is not equal");
            Assert.AreEqual(transactionModel.DestinationAccount, updatedTransaction.DestinationAccount, "Destination account is not equal");
            Assert.AreEqual(transactionModel.BankAccountId, updatedTransaction.BankAccountId, "Bank account id is not equal");
            Assert.AreEqual(transactionModel.Amount, updatedTransaction.Amount, "Amount is not equal");

            decimal expectedBankBalance = TestData.DefaultCheckingBankAccount.StartBalance + transactionModel.Amount;
            Assert.AreEqual(expectedBankBalance, SessionHelper.BankAccounts.Sum(item => item.CurrentBalance), "Incorrect current balance");
        }

        /// <summary>
        /// Validates if trying to update a transaction of another user fails
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 12)]
        public void UpsertTransaction_12_UpdateTransactionNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();

            Transaction transaction = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id);
            TransactionModel transactionModel = TransactionModel.CreateTransactionModel(transaction);

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index: 1);
            SessionHelper.TestUserId = otherUser.Id;

            // Act
            ActionResult result = accountOverviewController.UpsertTransaction(transactionModel);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult partialViewResult = (PartialViewResult)result;
            Assert.AreEqual("UpsertTransactionPartial", partialViewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(partialViewResult.Model, "Model should not be null");

            ControllerTestHelper.AssertTemporaryMessage(accountOverviewController.TempData, MessageState.Error, Finances.Resources.BankAccountNotFound);
        }

        /// <summary>
        /// Validates if an invalid model for upsert transaction returns the expected result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 13)]
        public void UpsertTransaction_13_InvalidModel_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();

            Transaction transaction = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id);
            TransactionModel transactionModel = TransactionModel.CreateTransactionModel(transaction);

            accountOverviewController.ModelState.AddModelError("", "Error");

            // Act
            ActionResult result = accountOverviewController.UpsertTransaction(transactionModel);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult partialViewResult = (PartialViewResult)result;
            Assert.AreEqual("UpsertTransactionPartial", partialViewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(partialViewResult.Model, "Model should not be null");
        }

        /// <summary>
        /// Validates if trying to update the category for a transaction fails when the user has no access to the transaction
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 14)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void QuickChangeTransactionCategory_14_InvalidTransactionIdNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertUserBankAccountTransactionCategory();

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index: 1);
            Category otherUserCategory = TestData.UpsertCategory(otherUser.Id, 1);

            SessionHelper.TestUserId = otherUser.Id;

            // Act
            ActionResult result = accountOverviewController.QuickChangeTransactionCategory(transactionId: TestData.DefaultTransaction.Id,
                                                                                            categoryId: otherUserCategory.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.AreEqual(false, jsonData.Success, "Success should be false");
            Assert.AreEqual(Finances.Resources.CouldNotUpdateTransactionCategory, jsonData.ErrorMessage, "Incorrect message returned");

            Transaction transaction = accountOverviewController.GenericRepository.FindSingle<Transaction>(item => item.Id == TestData.DefaultTransaction.Id,
                                                                                                            includeProperties: "TransactionCategories");

            Assert.IsFalse(transaction.TransactionCategories.Any(), "No transaction category should be assigned");
        }

        /// <summary>
        /// Validates if trying to update the category for a transaction fails when the user has no access to the category
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 15)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void QuickChangeTransactionCategory_15_InvalidCategoryIdNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertUserBankAccountTransactionCategory();

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index: 1);
            BankAccount otherBankAccount = TestData.UpsertBankAccount(userId: otherUser.Id, index: 1);
            Transaction otherUserTransaction = TestData.UpsertTransaction(bankAccountId: otherBankAccount.Id, applicationUserId: otherUser.Id, index: 1);

            SessionHelper.TestUserId = otherUser.Id;

            // Act
            ActionResult result = accountOverviewController.QuickChangeTransactionCategory(transactionId: otherUserTransaction.Id,
                                                                                            categoryId: TestData.DefaultCategory.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;
            Assert.AreEqual(false, jsonData.Success, "Success should be false");
            Assert.AreEqual(Finances.Resources.CouldNotUpdateTransactionCategory, jsonData.ErrorMessage, "Incorrect message returned");

            Transaction transaction = accountOverviewController.GenericRepository.FindSingle<Transaction>(item => item.Id == TestData.DefaultTransaction.Id,
                                                                                                            includeProperties: "TransactionCategories");

            Assert.IsFalse(transaction.TransactionCategories.Any(), "No transaction category should be assigned");
        }

        /// <summary>
        /// Validates if quick changing the category when there is no category returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 16)]
        public void QuickChangeTransactionCategory_16_InsertNewCategory_Success()
        {
            // Arrange
            TestData.UpsertUserBankAccountTransactionCategory();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = accountOverviewController.QuickChangeTransactionCategory(transactionId: TestData.DefaultTransaction.Id,
                                                                                            categoryId: TestData.DefaultCategory.Id);

            // Arrange
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult partialViewResult = (PartialViewResult)result;
            Assert.AreEqual("TransactionEditRow", partialViewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(partialViewResult.Model, "Model should not be null");
            Assert.IsInstanceOf<Transaction>(partialViewResult.Model, "Model is of incorrect type");

            Transaction transaction = (Transaction)partialViewResult.Model;
            Assert.AreEqual(TestData.DefaultTransaction.Id, transaction.Id, "Incorrect transaction returned");
            Assert.AreEqual(1, transaction.TransactionCategories.Count, "Incorrect number of categories");
            Assert.AreEqual(transaction.TransactionCategories.First().CategoryId, TestData.DefaultCategory.Id, "Incorrect transaction mapped");
        }

        /// <summary>
        /// Validates if quick changing the category when there is already a category returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 17)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void QuickChangeTransactionCategory_17_ChangeCategory_Success()
        {
            // Arrange
            TestData.UpsertUserBankAccountTransactionCategory();
            Category secondCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 1);
            TransactionCategory transactionCategoryOne = TestData.UpsertTransactionCategory(categoryId: TestData.DefaultCategory.Id,
                transactionId: TestData.DefaultTransaction.Id,
                index: 0,
                amount: TestData.DefaultTransaction.Amount / 2);
            TransactionCategory transactionCategoryTwo = TestData.UpsertTransactionCategory(categoryId: secondCategory.Id,
                transactionId: TestData.DefaultTransaction.Id,
                index: 1,
                amount: TestData.DefaultTransaction.Amount / 2);

            Category newCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 2);

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = accountOverviewController.QuickChangeTransactionCategory(transactionId: TestData.DefaultTransaction.Id,
                                                                                            categoryId: newCategory.Id);

            IList<TransactionCategory> oldTransactionCategories =
                accountOverviewController.GenericRepository.FindAll<TransactionCategory>(item => item.Id == transactionCategoryOne.Id ||
                                                                                             item.Id == transactionCategoryTwo.Id);

            // Arrange
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult partialViewResult = (PartialViewResult)result;
            Assert.AreEqual("TransactionEditRow", partialViewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(partialViewResult.Model, "Model should not be null");
            Assert.IsInstanceOf<Transaction>(partialViewResult.Model, "Model is of incorrect type");

            Transaction transaction = (Transaction)partialViewResult.Model;
            Assert.AreEqual(TestData.DefaultTransaction.Id, transaction.Id, "Incorrect transaction returned");
            Assert.AreEqual(1, transaction.TransactionCategories.Count, "Incorrect number of categories");
            Assert.AreEqual(transaction.TransactionCategories.First().CategoryId, newCategory.Id, "Incorrect transaction mapped");

            Assert.IsFalse(oldTransactionCategories.Any(), "Old transaction categories should be deleted");
        }

        /// <summary>
        /// Validates if quick removing the category for a transaction with an invalid transaction id fails correctly
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 18)]
        public void QuickRemoveTransactionCategory_18_ValidTransactionId_Success()
        {
            // Arrange
            TestData.UpsertUserBankAccountTransactionCategory();
            TransactionCategory upsertedTransactionCategory = TestData.UpsertTransactionCategory(TestData.DefaultCategory.Id,
                                                                                                    TestData.DefaultTransaction.Id);

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = accountOverviewController.QuickRemoveTransactionCategory(transactionId: TestData.DefaultTransaction.Id);

            // Arrange
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<PartialViewResult>(result, "Result is of incorrect type");

            PartialViewResult partialViewResult = (PartialViewResult)result;
            Assert.AreEqual("TransactionEditRow", partialViewResult.ViewName, "Incorrect view name");
            Assert.IsNotNull(partialViewResult.Model, "Model should not be null");
            Assert.IsInstanceOf<Transaction>(partialViewResult.Model, "Model is of incorrect type");

            Transaction transaction = (Transaction)partialViewResult.Model;
            Assert.AreEqual(TestData.DefaultTransaction.Id, transaction.Id, "Incorrect transaction returned");
            Assert.IsFalse(transaction.TransactionCategories.Any(), "No categories should be assigned");

            Assert.IsFalse(accountOverviewController.GenericRepository.FindAll<TransactionCategory>(item => item.Id == upsertedTransactionCategory.Id).Any(),
                message: "Transaction category was not deleted from database");
        }

        /// <summary>
        /// Validates if quick removing the category for a transaction with an invalid transaction id fails correctly
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 19)]
        public void QuickRemoveTransactionCategory_19_InvalidTransactionId_Fail()
        {
            // Arrange
            int transactionId = TestData.GenerateTestId(FinanceEntityType.Transaction);

            // Act
            ActionResult result = accountOverviewController.QuickRemoveTransactionCategory(transactionId: transactionId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.AreEqual(false, jsonData.Success, "Success should be false");
            Assert.AreEqual(Finances.Resources.CouldNotUpdateTransactionCategory, jsonData.ErrorMessage, "Incorrect message returned");
        }

        /// <summary>
        /// Validates if trying to remove the category for a transaction fails when the user has no access to the transaction
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [FinanceTestCase(FinanceTestCategory.AccountOverviewController, 20)]
        public void QuickRemoveTransactionCategory_20_InvalidTransactionIdNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertUserBankAccountTransactionCategory();
            TransactionCategory upsertedTransactionCategory = TestData.UpsertTransactionCategory(TestData.DefaultCategory.Id,
                                                                                                    TestData.DefaultTransaction.Id);

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index: 1);

            SessionHelper.TestUserId = otherUser.Id;

            // Act
            ActionResult result = accountOverviewController.QuickRemoveTransactionCategory(transactionId: TestData.DefaultTransaction.Id);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.AreEqual(false, jsonData.Success, "Success should be false");
            Assert.AreEqual(Finances.Resources.CouldNotUpdateTransactionCategory, jsonData.ErrorMessage, "Incorrect message returned");

            Assert.IsNotNull(accountOverviewController.GenericRepository.FindSingle<TransactionCategory>(item => item.Id == upsertedTransactionCategory.Id),
                message: "Transaction category was deleted from database");
        }

        #endregion

        #region Contract tests

        /// <summary>
        /// Contract test for the AddTransaction method
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void UpsertTransaction_ContractTest_NullModel()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                accountOverviewController.UpsertTransaction(null);
            });
        }

        #endregion
    }
}

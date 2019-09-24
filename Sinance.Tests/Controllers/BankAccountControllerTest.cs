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
    /// Test class for the bank account controller
    /// </summary>
    [TestFixture]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Objects are disposed in TestCleanup")]
    public class BankAccountControllerTest : FinanceTestBase<BankAccountControllerTestData>
    {
        #region Private Declarations

        private BankAccountController bankAccountController;
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

            bankAccountController = new BankAccountController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            bankAccountController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                bankAccountController);
        }

        /// <summary>
        /// Test cleanup
        /// </summary>
        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();

            bankAccountController.GenericRepository = null;
            bankAccountController.Dispose();
            bankAccountController = null;
        }

        #endregion

        #region Logical tests

        /// <summary>
        /// Validates that the Index action returns a Index view
        /// </summary>
        [TestCase(Author = "Roel de Wit")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 1)]
        public void Index_01_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            BankAccount bankAccountOne = TestData.UpsertBankAccount(userId: user.Id, index: 1);
            BankAccount bankAccountTwo = TestData.UpsertBankAccount(userId: user.Id, index: 2);

            SessionHelper.TestUserId = user.Id;

            // Act
            ViewResult result = bankAccountController.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< IList<BankAccount>>(result.Model, "Wrong model type");

            IList<BankAccount> bankAccountResult = (IList<BankAccount>)result.Model;
            Assert.AreEqual(2, bankAccountResult.Count, "Incorrect number of bank accounts returned");
            Assert.IsTrue(bankAccountResult.Any(item => item.Id == bankAccountOne.Id), "Bank account one not found");
            Assert.IsTrue(bankAccountResult.Any(item => item.Id == bankAccountTwo.Id), "Bank account two not found");
        }

        /// <summary>
        /// Validates if addaccount returns the expected result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 2)]
        public void AddAccount_02_Success()
        {
            // Act
            ViewResult result = bankAccountController.AddAccount() as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< BankAccountModel>(result.Model, "Wrong model type");

            BankAccountModel bankAccountResult = (BankAccountModel)result.Model;
            Assert.AreEqual(expected: 0, actual: bankAccountResult.Id, message: "Id was initialized incorrectly");
            Assert.IsNull(bankAccountResult.Name, message: "name was initialized incorrectly");
            Assert.AreEqual(expected: 0, actual: bankAccountResult.StartBalance, message: "StartBalance was initialized incorrectly");

        }

        /// <summary>
        /// Validates if Remove account executes the right actions
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",Justification = "Method is fine")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 3)]
        public void RemoveAccount_03_ValidAccountId_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser(index: 0);
            BankAccount bankAccount = TestData.UpsertBankAccount(userId: user.Id);
            Transaction transaction = TestData.UpsertTransaction(applicationUserId: user.Id,
                                                                    bankAccountId: bankAccount.Id);
            SessionHelper.TestUserId = user.Id;

            // Act
            RedirectToRouteResult result = bankAccountController.RemoveAccount(bankAccount.Id) as RedirectToRouteResult;

            BankAccount deletedAccount = bankAccountController.GenericRepository.FindSingle<BankAccount>(item => item.Id == bankAccount.Id);
            Transaction deletedTransaction = bankAccountController.GenericRepository.FindSingle<Transaction>(item => item.Id == transaction.Id);
            
            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");
            Assert.IsFalse(SessionHelper.BankAccounts.Any(), "No bank accounts should be available");
            ControllerTestHelper.AssertTemporaryMessage(bankAccountController.TempData, MessageState.Success, Finances.Resources.BankAccountRemoved);

            Assert.IsNull(deletedAccount, "Account was not deleted");
            Assert.IsNull(deletedTransaction, "Transactions werent deleted");
        }

        /// <summary>
        /// Validates if Remove account executes the right actions incase of an invalid account id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 4)]
        public void RemoveAccount_04_InvalidAccountId_Fail()
        {
            // Arrange
            int invalidBankAccountId = TestData.GenerateTestId(FinanceEntityType.BankAccount);

            //Act
            RedirectToRouteResult result = bankAccountController.RemoveAccount(invalidBankAccountId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");

            ControllerTestHelper.AssertTemporaryMessage(bankAccountController.TempData, MessageState.Error, Finances.Resources.BankAccountNotFound);
        }

        /// <summary>
        /// Validates if Edit account executes the right actions incase of a valid account id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 5)]
        public void EditAccount_05_ValidAccountId_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            BankAccount bankAccount = TestData.UpsertBankAccount(userId: user.Id);

            SessionHelper.TestUserId = user.Id;

            // Act
            ViewResult result = bankAccountController.EditAccount(bankAccount.Id) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< BankAccountModel>(result.Model, "Model is incorrect type");

            BankAccountModel editBankAccount = (BankAccountModel)result.Model;
            Assert.AreEqual(bankAccount.Id, editBankAccount.Id, "Incorrect model returned");
            Assert.AreEqual(bankAccount.Name, editBankAccount.Name, "Incorrect model returned");
            Assert.AreEqual(bankAccount.CurrentBalance, editBankAccount.CurrentBalance, "Incorrect model returned");
            Assert.AreEqual(bankAccount.StartBalance, editBankAccount.StartBalance, "Incorrect model returned");
        }

        /// <summary>
        /// Validates if Edit account executes the right actions incase of an invalid account id
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 6)]
        public void EditAccount_06_InvalidAccountId_Fail()
        {
            // Arrange
            int invalidBankAccountId = TestData.GenerateTestId(FinanceEntityType.BankAccount);

            // Act
            RedirectToRouteResult result = bankAccountController.EditAccount(invalidBankAccountId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");
            ControllerTestHelper.AssertTemporaryMessage(bankAccountController.TempData, MessageState.Error, Finances.Resources.BankAccountNotFound);
        }

        /// <summary>
        /// Validates if upserting a new account completes succesfully
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 7)]
        public void UpsertAccount_07_AddNewAccount_Success()
        {
            // Arrange
            ApplicationUser applicationUser = TestData.UpsertApplicationUser();
            BankAccount bankAccount = TestData.CreateBankAccount(id: 0, index: 0, startBalance: 100);
            BankAccountModel model = BankAccountControllerTestData.CreateBankAccountModel(bankAccount);
            
            // Set the current test user id to the created user
            SessionHelper.TestUserId = applicationUser.Id;

            // Act
            RedirectToRouteResult result = bankAccountController.UpsertAccount(model: model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");
            ControllerTestHelper.AssertTemporaryMessage(bankAccountController.TempData, MessageState.Success, Finances.Resources.BankAccountCreated);

            // Find the created bank account, current balance must be set as well!
            bool accountExists = SessionHelper.BankAccounts.Any(item => item.Name == model.Name && 
                                                                item.StartBalance == model.StartBalance &&
                                                                item.CurrentBalance == model.StartBalance);

            Assert.IsTrue(accountExists, "Expected bank account not found in session");
            Assert.AreEqual(1, SessionHelper.BankAccounts.Count, "Incorrect number of bank accounts available");
        }

        /// <summary>
        /// Validates if upserting a new account with the same name fails as expected
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 8)]
        public void UpsertAccount_08_AddNewAccount_SameName_Fail()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            BankAccount existingBankAccount = TestData.UpsertBankAccount(userId: user.Id, startBalance: 100);
            BankAccount newBankAccount = TestData.CreateBankAccount(id: 0, index:1, startBalance: 200, name: existingBankAccount.Name);
            BankAccountModel model = BankAccountControllerTestData.CreateBankAccountModel(newBankAccount);

            // Act
            ViewResult result = bankAccountController.UpsertAccount(model: model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< BankAccountModel>(result.Model, "Model is incorrect type");
            Assert.AreEqual(Finances.Resources.BankAccountAlreadyExists, bankAccountController.ModelState["Message"].Errors.First().ErrorMessage, "Incorrect message");
        }

        /// <summary>
        /// Validates if editing an existing account executes the expected actions
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "All are needed")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 9)]
        public void UpsertAccount_09_UpdateAccount_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            BankAccount existingBankAccount = TestData.UpsertBankAccount(userId: user.Id, startBalance: 100, currentBalance: 100);
            Transaction transaction = TestData.UpsertTransaction(applicationUserId:user.Id, bankAccountId: existingBankAccount.Id, amount: 500);

            // Set the current user test id
            SessionHelper.TestUserId = user.Id;

            BankAccount updatedBankAccount = TestData.CreateBankAccount(id: existingBankAccount.Id,
                startBalance: 200,
                currentBalance: 300,
                name: string.Format(CultureInfo.CurrentCulture, "{0}_2", existingBankAccount.Name));
            BankAccountModel model = BankAccountControllerTestData.CreateBankAccountModel(updatedBankAccount);

            // Act
            RedirectToRouteResult result = bankAccountController.UpsertAccount(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");
            ControllerTestHelper.AssertTemporaryMessage(bankAccountController.TempData, MessageState.Success, Finances.Resources.BankAccountUpdated);

            BankAccount savedAccount = SessionHelper.BankAccounts.SingleOrDefault(item => item.Id == existingBankAccount.Id);

            Assert.IsNotNull(savedAccount, "Bank account is not present in the session");
            Assert.AreEqual(updatedBankAccount.StartBalance, savedAccount.StartBalance, "Start balance was not updated correctly");
            Assert.AreEqual(savedAccount.StartBalance + transaction.Amount, savedAccount.CurrentBalance, "Current balance should not be updated");
            Assert.AreEqual(updatedBankAccount.Name, savedAccount.Name, "name was not updated correctly");
            Assert.AreEqual(1, SessionHelper.BankAccounts.Count, "Incorrect number of bank accounts returned");
        }
        
        /// <summary>
        /// Validates if trying to Upsert an account which does not exist fails
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 10)]
        public void UpsertAccount_10_UpdateAccount_InvalidId_Fail()
        {
            // Arrange
            TestData.UpsertApplicationUser();
            BankAccount updatedBankAccount = TestData.CreateBankAccount(id: TestData.GenerateTestId(FinanceEntityType.BankAccount), startBalance: 100);
            BankAccountModel model = BankAccountControllerTestData.CreateBankAccountModel(updatedBankAccount);

            // Act
            ViewResult result = bankAccountController.UpsertAccount(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< BankAccountModel>(result.Model, "Model is incorrect type");
            Assert.AreEqual(Finances.Resources.BankAccountNotFound, bankAccountController.ModelState["Message"].Errors.First().ErrorMessage, "Incorrect message");
        }

        /// <summary>
        /// Validates if trying to upsert with an invalid model fails
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 11)]
        public void UpsertAccount_11_InvalidModel_Fail()
        {
            // Arrange
            TestData.UpsertApplicationUser();
            BankAccount invalidBankAccount = TestData.CreateBankAccount(id: TestData.GenerateTestId(FinanceEntityType.BankAccount, 1), startBalance: 100);
            BankAccountModel model = BankAccountControllerTestData.CreateBankAccountModel(invalidBankAccount);

            bankAccountController.ModelState.AddModelError("Error", new ArgumentException("name"));

            // Act
            ViewResult result = bankAccountController.UpsertAccount(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.IsNotNull(result.Model, "No model returned");
            Assert.IsInstanceOf< BankAccountModel>(result.Model, "Model is incorrect type");
        }

        /// <summary>
        /// Validates if setting an account disabled saves correctly
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.BankAccountController, 12)]
        public void UpsertAccount_12_DisableAccount_Success()
        {
            // Arrange
            ApplicationUser user = TestData.UpsertApplicationUser();
            BankAccount existingBankAccount = TestData.UpsertBankAccount(userId: user.Id, startBalance: 100, currentBalance: 100);

            // Set the current user test id
            SessionHelper.TestUserId = user.Id;

            BankAccount updatedBankAccount = TestData.CreateBankAccount(id: existingBankAccount.Id,
                startBalance: 200,
                currentBalance: 300,
                disabled: false,
                name: string.Format(CultureInfo.CurrentCulture, "{0}_2", existingBankAccount.Name));
            BankAccountModel model = BankAccountControllerTestData.CreateBankAccountModel(updatedBankAccount);
            model.Disabled = true;

            // Act
            RedirectToRouteResult result = bankAccountController.UpsertAccount(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "No result returned");
            Assert.AreEqual("Index", result.RouteValues["action"], "Incorrect action");
            ControllerTestHelper.AssertTemporaryMessage(bankAccountController.TempData, MessageState.Success, Finances.Resources.BankAccountUpdated);

            BankAccount savedAccount = SessionHelper.BankAccounts.SingleOrDefault(item => item.Id == existingBankAccount.Id);

            Assert.AreEqual(true, savedAccount.Disabled, "Disabled should be set to true");
        }

        #endregion

        #region Contract tests

        /// <summary>
        /// Contract test for the upsert account method
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void UpsertAccount_ContractTest_NullModel()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                bankAccountController.UpsertAccount(model: null);
            });
        }

        /// <summary>
        /// Contract test for the remove account method
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void RemoveAccount_ContractTest_InvalidAccountId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                bankAccountController.RemoveAccount(accountId: 0);
            });
        }

        /// <summary>
        /// Contract test for the edit account method
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        public void EditAccount_ContractTest_InvalidAccountId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                bankAccountController.EditAccount(accountId: 0);
            });
        }

        #endregion
    }
}

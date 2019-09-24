using System.Diagnostics.CodeAnalysis;
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
    /// Test class for the home controller
    /// </summary>
    [TestFixture]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class HomeControllerTest : FinanceTestBase<FinanceTestData>
    {
        #region Private Declarations

        private HomeController homeController;
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

            homeController = new HomeController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            homeController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                homeController);
        }

        /// <summary>
        /// Test cleanup
        /// </summary>
        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();

            homeController.GenericRepository = null;
            homeController.Dispose();
            homeController = null;
        }

        #endregion

        #region Logical Tests

        /// <summary>
        /// Validates if the index action returns the correct bank accounts
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.HomeController, 1)]
        public void Index_01_BankAccounts_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = homeController.Index();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Incorrect view type returned");

            ViewResult viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOf< DashboardModel>(viewResult.Model, "Incorrect model type");

            DashboardModel model = (DashboardModel)viewResult.Model;
            Assert.AreEqual(1, model.CheckingBankAccounts.Count, "Invalid number of bank accounts");

            BankAccount returnedBankAccount = model.CheckingBankAccounts.First();
            Assert.AreEqual(TestData.DefaultCheckingBankAccount.Id, returnedBankAccount.Id, "Incorrect bank account returned");
            Assert.AreEqual(TestData.DefaultCheckingBankAccount.Name, returnedBankAccount.Name, "Incorrect name returned");
            Assert.AreEqual(TestData.DefaultCheckingBankAccount.CurrentBalance, returnedBankAccount.CurrentBalance, "Incorrect CurrentBalance returned");
            Assert.AreEqual(TestData.DefaultCheckingBankAccount.StartBalance, returnedBankAccount.StartBalance, "Incorrect StartBalance returned");
        }

        #endregion
    }
}

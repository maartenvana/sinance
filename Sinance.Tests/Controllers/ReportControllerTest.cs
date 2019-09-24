using System;
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
    /// Test class for the report controller
    /// </summary>
    [TestFixture]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ReportControllerTest : FinanceTestBase<FinanceTestData>
    {
        #region Private Declarations

        private ReportController reportController;
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

            reportController = new ReportController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            reportController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                reportController);
        }

        /// <summary>
        /// Test cleanup
        /// </summary>
        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();

            reportController.GenericRepository = null;
            reportController.Dispose();
            reportController = null;
        }

        #endregion

        #region Logical tests

        /// <summary>
        /// Validates if the expense overview action returns the correct result when there is no data
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 1)]
        public void ExpenseOverview_01_NoCategoriesNoTransactions_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult) result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel) viewResult.Model;
            Assert.IsNotNull(modelResult.RegularBimonthlyExpenseReport, "Regular expenses should not be null");
            Assert.IsNotNull(modelResult.VolatileBimonthlyExpenseReport, "Volatile expenses should not be null");

            Assert.IsFalse(modelResult.RegularBimonthlyExpenseReport.Expenses.Any(), "Regular expenses should not contain anything");
            Assert.IsFalse(modelResult.VolatileBimonthlyExpenseReport.Expenses.Any(), "Volatile expenses should not contain anything");
        }

        /// <summary>
        /// Validates if the expense overview action returns the correct result
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 2)]
        public void ExpenseOverview_02_ValidResult_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Upsert the parent categories
            Category regularParentCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 0, isRegular: true);
            Category volatileParentCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 1, isRegular: false);

            // Upsert the categories who will be actually used for transactions
            Category regularCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 2, isRegular: false, parentId:regularParentCategory.Id);
            Category volatileCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 3, isRegular: false, parentId:volatileParentCategory.Id);

            // Regular transactions
            Transaction lastMonthRegularTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id, 
                TestData.DefaultTestApplicationUser.Id, index: 0, date: DateTime.Now.AddMonths(-1), amount: -35);
            Transaction thisMonthRegularTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 1, date: DateTime.Now.AddDays(-1), amount: -45);

            // Volatile transactions
            Transaction lastMonthVolatileTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 2, date: DateTime.Now.AddMonths(-1), amount: -65);
            Transaction thisMonthVolatileTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 3, date: DateTime.Now.AddDays(-1), amount: -55);

            // Upsert links to categories
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: lastMonthRegularTransaction.Id, index: 0);
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: thisMonthRegularTransaction.Id, index: 1);
            TestData.UpsertTransactionCategory(volatileCategory.Id, transactionId: lastMonthVolatileTransaction.Id, index: 2);
            TestData.UpsertTransactionCategory(volatileCategory.Id, transactionId: thisMonthVolatileTransaction.Id, index: 3);
            
            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel)viewResult.Model;

            Assert.AreEqual(1, modelResult.RegularBimonthlyExpenseReport.Expenses.Count, "Invalid amount of regular parent expenses");
            Assert.AreEqual(1, modelResult.VolatileBimonthlyExpenseReport.Expenses.Count, "Invalid amount of volatile parent expenses");

            Assert.AreEqual(1, modelResult.RegularBimonthlyExpenseReport.Expenses.First().ChildBimonthlyExpenses.Count, "Invalid amount of regular child expenses");
            Assert.AreEqual(1, modelResult.VolatileBimonthlyExpenseReport.Expenses.First().ChildBimonthlyExpenses.Count, "Invalid amount of volatile child expenses");

            BimonthlyExpense regularMonthlyExpenseParent = modelResult.RegularBimonthlyExpenseReport.Expenses.Single(item => 
                item.Name == regularParentCategory.Name);
            BimonthlyExpense volatileMonthlyExpenseParent = modelResult.VolatileBimonthlyExpenseReport.Expenses.Single(item => 
                item.Name == volatileParentCategory.Name);

            BimonthlyExpense regularMonthlyChildExpense = regularMonthlyExpenseParent.ChildBimonthlyExpenses.Single(item =>
                item.Name == regularCategory.Name);
            BimonthlyExpense volatileMonthlyChildExpense = volatileMonthlyExpenseParent.ChildBimonthlyExpenses.Single(item =>
                item.Name == volatileCategory.Name);

            AssertMonthlyExpense(expense: regularMonthlyExpenseParent, 
                category: regularParentCategory,
                amountLastMonth: regularMonthlyChildExpense.AmountPrevious, 
                amountThisMonth: regularMonthlyChildExpense.AmountNow);

            AssertMonthlyExpense(expense: volatileMonthlyExpenseParent, 
                category: volatileParentCategory,
                amountLastMonth: volatileMonthlyChildExpense.AmountPrevious, 
                amountThisMonth: volatileMonthlyChildExpense.AmountNow);

            AssertMonthlyExpense(expense: regularMonthlyChildExpense, 
                category: regularCategory, 
                amountLastMonth: lastMonthRegularTransaction.Amount, 
                amountThisMonth: thisMonthRegularTransaction.Amount);

            AssertMonthlyExpense(expense: volatileMonthlyChildExpense, 
                category: volatileCategory,
                amountLastMonth: lastMonthVolatileTransaction.Amount, 
                amountThisMonth: thisMonthVolatileTransaction.Amount);
        }

        /// <summary>
        /// Validates if positive transactions do not appear in the expense overview
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 3)]
        public void ExpenseOverview_03_NoPositiveTransactions_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Upsert the categories who will be actually used for transactions
            Category regularCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 0, isRegular: true);
            Category volatileCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 1, isRegular: false);

            // Regular transactions
            Transaction lastMonthRegularTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 0, date: DateTime.Now.AddMonths(-1), amount: 35);

            // Volatile transactions
            Transaction lastMonthVolatileTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 1, date: DateTime.Now.AddMonths(-1), amount: 65);

            // Upsert links to categories
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: lastMonthRegularTransaction.Id, index: 0);
            TestData.UpsertTransactionCategory(volatileCategory.Id, transactionId: lastMonthVolatileTransaction.Id, index: 1);

            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel)viewResult.Model;
            Assert.AreEqual(1, modelResult.RegularBimonthlyExpenseReport.Expenses.Count, "Invalid amount of regular expenses");
            Assert.AreEqual(1, modelResult.VolatileBimonthlyExpenseReport.Expenses.Count, "Invalid amount of volatile expenses");

            BimonthlyExpense regularMonthlyExpenseParent = modelResult.RegularBimonthlyExpenseReport.Expenses.Single(item => 
                item.Name == regularCategory.Name);
            BimonthlyExpense volatileMonthlyExpenseParent = modelResult.VolatileBimonthlyExpenseReport.Expenses.Single(item => 
                item.Name == volatileCategory.Name);

            AssertMonthlyExpense(expense: regularMonthlyExpenseParent, category: regularCategory, 
                amountLastMonth: 0, amountThisMonth: 0);
            AssertMonthlyExpense(expense: volatileMonthlyExpenseParent, category: volatileCategory, 
                amountLastMonth: 0, amountThisMonth: 0);
        }

        /// <summary>
        /// Validates if the expense overview returns the correct result when there is no record for the previous month
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 4)]
        public void ExpenseOverview_04_NewRegularExpenseNoPreviousTransaction_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Upsert the parent categories
            Category regularParentCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 0, isRegular: true);
            
            // Upsert the categories who will be actually used for transactions
            Category regularCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 2, isRegular: false, parentId: regularParentCategory.Id);
            
            // Regular transactions
            Transaction thisMonthRegularTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 1, date: DateTime.Now.AddDays(-1), amount: -45);

            // Upsert links to categories
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: thisMonthRegularTransaction.Id, index: 1);
            
            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel)viewResult.Model;
            Assert.AreEqual(0, modelResult.RegularBimonthlyExpenseReport.RemainingAmount, "Incorrect remaining amount");
            Assert.AreEqual(0, modelResult.RegularBimonthlyExpenseReport.PreviousMonthTotal, "Incorrect previous month total");
            Assert.AreEqual(thisMonthRegularTransaction.Amount, modelResult.RegularBimonthlyExpenseReport.ThisMonthTotal, "Incorrect current month total");
        }


        /// <summary>
        /// Validates if the expense overview returns the correct result when there are more records for the previous month
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 5)]
        public void ExpenseOverview_05_NewRegularExpenseMorePreviousTransactions_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Upsert the parent categories
            Category regularParentCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 0, isRegular: true);

            // Upsert the categories who will be actually used for transactions
            Category regularCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 2, isRegular: false, parentId: regularParentCategory.Id);

            // Regular transactions for last month
            Transaction lastMonthRegularTransactionOne = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 1, date: DateTime.Now.AddMonths(-1), amount: -45);
            Transaction lastMonthRegularTransactionTwo = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 2, date: DateTime.Now.AddMonths(-1), amount: -60);

            // Regular transactions for this month
            Transaction thisMonthRegularTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 3, date: DateTime.Now, amount: -45);

            // Upsert links to categories
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: lastMonthRegularTransactionOne.Id, index: 1);
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: lastMonthRegularTransactionTwo.Id, index: 2);
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: thisMonthRegularTransaction.Id, index: 3);

            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel)viewResult.Model;
            Assert.AreEqual(lastMonthRegularTransactionTwo.Amount, modelResult.RegularBimonthlyExpenseReport.RemainingAmount, "Incorrect remaining amount");
            Assert.AreEqual(lastMonthRegularTransactionOne.Amount + lastMonthRegularTransactionTwo.Amount,
                modelResult.RegularBimonthlyExpenseReport.PreviousMonthTotal, "Incorrect previous month total");
            Assert.AreEqual(thisMonthRegularTransaction.Amount, modelResult.RegularBimonthlyExpenseReport.ThisMonthTotal, "Incorrect current month total");
        }

        /// <summary>
        /// Validates if the uncategorized transactions are set as expected
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 6)]
        public void ExpenseOverview_06_ExpensesWithoutCategories_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Regular transactions for last month
            TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 0, date: DateTime.Now.AddMonths(-1), amount: -45);

            Transaction thisMonthTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 1, date: DateTime.Now, amount: -45);

            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel)viewResult.Model;
            Assert.AreEqual(0, modelResult.RegularBimonthlyExpenseReport.RemainingAmount, "Incorrect remaining amount");
            Assert.AreEqual(0, modelResult.RegularBimonthlyExpenseReport.PreviousMonthTotal, "Incorrect previous month total");
            Assert.AreEqual(0, modelResult.RegularBimonthlyExpenseReport.ThisMonthTotal, "Incorrect current month total");
            Assert.AreEqual(1, modelResult.UncategorizedTransactionsThisMonth.Count(), "Incorrect number of transactions");
            Assert.AreEqual(thisMonthTransaction.Id, modelResult.UncategorizedTransactionsThisMonth.First().Id, "Incorrect transaction id returned");
            Assert.AreEqual(thisMonthTransaction.Name, modelResult.UncategorizedTransactionsThisMonth.First().Name, "Incorrect transaction name returned");
        }

        /// <summary>
        /// Validates if the uncategorized transactions are set as expected
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 7)]
        public void ExpenseOverview_07_ExpenseMultipleCategories_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Upsert the parent categories
            Category regularParentCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 0, isRegular: true);
            Category volatileParentCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 1, isRegular: false);

            // Upsert the categories who will be actually used for transactions
            Category regularCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 2, isRegular: false, parentId: regularParentCategory.Id);

            // Regular transactions for last month
            Transaction lastMonthTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 1, date: DateTime.Now.AddMonths(-1), amount: -35);
            
            // Upsert links to categories
            TransactionCategory transactionCategoryOne = TestData.UpsertTransactionCategory(categoryId: regularCategory.Id, 
                transactionId: lastMonthTransaction.Id, index: 1, amount: lastMonthTransaction.Amount/2);
            TransactionCategory transactionCategoryTwo = TestData.UpsertTransactionCategory(categoryId: volatileParentCategory.Id, 
                transactionId: lastMonthTransaction.Id, index: 2, amount: lastMonthTransaction.Amount / 2);

            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel)viewResult.Model;
            Assert.AreEqual(transactionCategoryOne.Amount, modelResult.RegularBimonthlyExpenseReport.RemainingAmount, "Incorrect remaining amount");
            Assert.AreEqual(transactionCategoryTwo.Amount, modelResult.VolatileBimonthlyExpenseReport.RemainingAmount, "Incorrect remaining amount");

            Assert.AreEqual(transactionCategoryOne.Amount, modelResult.RegularBimonthlyExpenseReport.PreviousMonthTotal, "Incorrect previous month total");
            Assert.AreEqual(transactionCategoryTwo.Amount, modelResult.VolatileBimonthlyExpenseReport.PreviousMonthTotal, "Incorrect previous month total");

            Assert.AreEqual(0, modelResult.RegularBimonthlyExpenseReport.ThisMonthTotal, "Incorrect current month total");
            Assert.AreEqual(0, modelResult.VolatileBimonthlyExpenseReport.ThisMonthTotal, "Incorrect current month total");
        }

        /// <summary>
        /// Validates if the expected result is returned when there are more transactions for this month
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.ReportController, 8)]
        public void ExpenseOverview_08_MoreTransactionsThisMonth_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Upsert the parent categories
            Category regularParentCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 0, isRegular: true);

            // Upsert the categories who will be actually used for transactions
            Category regularCategory = TestData.UpsertCategory(TestData.DefaultTestApplicationUser.Id, index: 2, isRegular: false, parentId: regularParentCategory.Id);

            // Regular transactions for last month
            Transaction lastMonthRegularTransactionOne = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 1, date: DateTime.Now.AddMonths(-1), amount: -45);
            
            // Regular transactions for this month
            Transaction thisMonthRegularTransactionOne = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 2, date: DateTime.Now, amount: -55);
            Transaction thisMonthRegularTransactionTwo = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id,
                TestData.DefaultTestApplicationUser.Id, index: 3, date: DateTime.Now, amount: -65);

            // Upsert links to categories
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: lastMonthRegularTransactionOne.Id, index: 1);
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: thisMonthRegularTransactionOne.Id, index: 2);
            TestData.UpsertTransactionCategory(regularCategory.Id, transactionId: thisMonthRegularTransactionTwo.Id, index: 3);

            // Act
            ActionResult result = reportController.ExpenseOverview();

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< ViewResult>(result, "Result is of incorrect type");

            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual("ExpenseOverview", viewResult.ViewName, "Incorrect view name returned");
            Assert.IsInstanceOf< ExpensesModel>(viewResult.Model, "Incorrect type of model returned");

            ExpensesModel modelResult = (ExpensesModel)viewResult.Model;
            Assert.AreEqual(0, modelResult.RegularBimonthlyExpenseReport.RemainingAmount, "Incorrect remaining amount");
            Assert.AreEqual(lastMonthRegularTransactionOne.Amount, modelResult.RegularBimonthlyExpenseReport.PreviousMonthTotal, "Incorrect previous month total");
            Assert.AreEqual(thisMonthRegularTransactionOne.Amount + thisMonthRegularTransactionTwo.Amount,
                modelResult.RegularBimonthlyExpenseReport.ThisMonthTotal, "Incorrect current month total");
        }

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Asserts the monthly expense for correct values
        /// </summary>
        /// <param name="expense">Monthly expense to assert</param>
        /// <param name="category">Category that should be set</param>
        /// <param name="amountLastMonth">Amount that was expended last month</param>
        /// <param name="amountThisMonth">Amount that was expended this month</param>
        private static void AssertMonthlyExpense(BimonthlyExpense expense, Category category,
            decimal amountLastMonth, decimal amountThisMonth)
        {
            Assert.AreEqual(category.Name, expense.Name, "Category name is incorrect for monthly expense");
            Assert.AreEqual(amountLastMonth, expense.AmountPrevious, "Incorrect amount previous set");
            Assert.AreEqual(amountThisMonth, expense.AmountNow, "Incorrect amount now set");
        }

        #endregion
    }
}
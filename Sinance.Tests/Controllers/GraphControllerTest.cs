using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Finances.Bll.Classes;
using Finances.Bll.Handlers;
using Finances.Controllers;
using Finances.Domain.Entities;
using Finances.UnitTestBase.Classes;
using Rhino.Mocks;
using NUnit.Framework;

namespace Finances.Web.Tests.Controllers
{
    /// <summary>
    /// Testclass for the graph controller
    /// </summary>
    [TestFixture]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class GraphControllerTest : FinanceTestBase<FinanceTestData>
    {
        #region Private Declarations

        private GraphController graphController;
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

            graphController = new GraphController();

            // Setup mock for the session
            httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
            httpSessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            httpContextBase.Stub(stub => stub.Session).Return(httpSessionStateBase);

            SessionHelper.HttpContextBase = httpContextBase;

            graphController.ControllerContext = new ControllerContext(httpContextBase, new RouteData(),
                graphController);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates if method BankAccountTransactions returns the correct result when there are no transactions available
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 1)]
        public void BankAccountTransactions_01_NoTransactionsAvailable_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            // Act
            ActionResult result = graphController.BalanceHistoryPerYear(DateTime.Now.Year, new[] { TestData.DefaultCheckingBankAccount.Id });

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult) ((JsonResult)result).Data;
            Assert.IsInstanceOf< List<decimal[]>>(jsonData.ObjectData, "Incorrect data type returned");

            IList<decimal[]> transactions = (List<decimal[]>)jsonData.ObjectData;
            Assert.IsFalse(transactions.Any(), "Transactions list should not contain any transactions");
        }

        /// <summary>
        /// Validates if method BankAccountTransactions returns the correct result when there are no previous year transaction available
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 2)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void BankAccountTransactions_02_NoPreviousYearTransactions_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            int currentYear = DateTime.Now.Year;
            DateTime firstExpectedDate = new DateTime(currentYear, 2, 2);
            DateTime secondExpectedDate = new DateTime(currentYear, 2, 3);
            DateTime startDateUtc = new DateTime(1970, 1, 1);

            decimal firstExpectedDateUtc = Convert.ToDecimal(firstExpectedDate.Subtract(startDateUtc).TotalMilliseconds);
            decimal secondExpectedDateUtc = Convert.ToDecimal(secondExpectedDate.Subtract(startDateUtc).TotalMilliseconds);
            
            Transaction thisYearTransactionOne = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id, 
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 0, date: firstExpectedDate);
            Transaction thisYearTransactionTwo = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id, 
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 1, date: firstExpectedDate);
            Transaction thisYearTransactionThree = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id, 
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 2, date: secondExpectedDate, amount:-100);

            // Act
            ActionResult result = graphController.BalanceHistoryPerYear(currentYear, new[] { TestData.DefaultCheckingBankAccount.Id });

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf<JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsTrue(jsonData.Success, "Success should be set to true");
            Assert.IsNotNull(jsonData.ObjectData, "Object data should be set");
            Assert.IsInstanceOf< List < decimal[]>>(jsonData.ObjectData, "Incorrect data type returned");

            IList<decimal[]> transactions = (List<decimal[]>)jsonData.ObjectData;
            Assert.AreEqual(2, transactions.Count, "Incorrect amount of transactions returned");

            decimal[] firstTransactionsGroup = transactions[0];
            decimal[] secondTransactionsGroup = transactions[1];

            Assert.AreEqual(firstExpectedDateUtc, firstTransactionsGroup[0], "first date is not correct");
            Assert.AreEqual(secondExpectedDateUtc, secondTransactionsGroup[0], "second date is not correct");

            Assert.AreEqual(expected: thisYearTransactionOne.Amount + thisYearTransactionTwo.Amount + TestData.DefaultCheckingBankAccount.StartBalance, 
                actual: firstTransactionsGroup[1], message: "Incorrect amount for first group");

            decimal expectedSecondGroupAmount = thisYearTransactionOne.Amount + thisYearTransactionTwo.Amount + TestData.DefaultCheckingBankAccount.StartBalance +
                                                thisYearTransactionThree.Amount;
            Assert.AreEqual(expected: expectedSecondGroupAmount, actual: secondTransactionsGroup[1], message: "Incorrect amount for second group");
        }

        /// <summary>
        /// Validates if method BankAccountTransactions returns the correct result when the request contains only a bank account of another user
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 3)]
        public void BankAccountTransactions_03_InvalidBankAccountNoAccess_Fail()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            ApplicationUser otherUser = TestData.UpsertApplicationUser(index: 1);
            BankAccount otherBankAccount = TestData.UpsertBankAccount(userId: otherUser.Id, index: 1);

            // Act
            ActionResult result = graphController.BalanceHistoryPerYear(DateTime.Now.Year, new[] { otherBankAccount.Id });

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsFalse(jsonData.Success, "Success should be set to false");
            Assert.AreEqual(Finances.Resources.Error, jsonData.ErrorMessage, "Incorrect error message");
            Assert.IsNull(jsonData.ObjectData, "Object data should not be set");
        }

        /// <summary>
        /// Validates if method BankAccountTransactions returns the correct result when there are previous year transactions available
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 4)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void BankAccountTransactions_04_PreviousTransactionsAvailable_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            int currentYear = DateTime.Now.Year;
            DateTime lastYearDate = new DateTime(currentYear - 1, 2, 2);
            DateTime firstExpectedDate = new DateTime(currentYear, 1, 1);
            DateTime secondExpectedDate = new DateTime(currentYear, 2, 2);
            DateTime thirdExpectedDate = new DateTime(currentYear, 2, 3);
            DateTime startDateUtc = new DateTime(1970, 1, 1);

            decimal firstExpectedDateUtc = Convert.ToDecimal(firstExpectedDate.Subtract(startDateUtc).TotalMilliseconds);
            decimal secondExpectedDateUtc = Convert.ToDecimal(secondExpectedDate.Subtract(startDateUtc).TotalMilliseconds);
            decimal thirdExpectedDateUtc = Convert.ToDecimal(thirdExpectedDate.Subtract(startDateUtc).TotalMilliseconds);

            Transaction lastYearTransaction = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 0, date: lastYearDate, amount:150);

            Transaction thisYearTransactionOne = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 1, date: secondExpectedDate, amount: 150);
            Transaction thisYearTransactionTwo = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 2, date: secondExpectedDate, amount: 150);
            Transaction thisYearTransactionThree = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 3, date: thirdExpectedDate, amount: -100);

            // Act
            ActionResult result = graphController.BalanceHistoryPerYear(currentYear, new[] { TestData.DefaultCheckingBankAccount.Id });

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsTrue(jsonData.Success, "Success should be set to true");
            Assert.IsNotNull(jsonData.ObjectData, "Object data should be set");
            Assert.IsInstanceOf< List<decimal[]>>(jsonData.ObjectData, "Incorrect data type returned");

            IList<decimal[]> transactions = (List<decimal[]>)jsonData.ObjectData;
            Assert.AreEqual(3, transactions.Count, "Incorrect amount of transactions returned");

            decimal[] firstTransactionsGroup = transactions[0];
            decimal[] secondTransactionsGroup = transactions[1];
            decimal[] thirdTransactionsGroup = transactions[2];

            Assert.AreEqual(firstExpectedDateUtc, firstTransactionsGroup[0], "first date is not correct");
            Assert.AreEqual(secondExpectedDateUtc, secondTransactionsGroup[0], "second date is not correct");
            Assert.AreEqual(thirdExpectedDateUtc, thirdTransactionsGroup[0], "third date is not correct");

            decimal expectedFirstGroupAmount = lastYearTransaction.Amount + TestData.DefaultCheckingBankAccount.StartBalance;
            Assert.AreEqual(expected: expectedFirstGroupAmount, actual: firstTransactionsGroup[1], message: "Incorrect amount for first group");
            
            decimal expectedSeecondGroupAmount = lastYearTransaction.Amount + thisYearTransactionOne.Amount + thisYearTransactionTwo.Amount +
                                               TestData.DefaultCheckingBankAccount.StartBalance;
            Assert.AreEqual(expected: expectedSeecondGroupAmount, actual: secondTransactionsGroup[1], message: "Incorrect amount for second group");

            decimal expectedThirdGroupAmount = lastYearTransaction.Amount + thisYearTransactionOne.Amount + thisYearTransactionTwo.Amount + 
                                                TestData.DefaultCheckingBankAccount.StartBalance + thisYearTransactionThree.Amount;
            Assert.AreEqual(expected: expectedThirdGroupAmount, actual: thirdTransactionsGroup[1], message: "Incorrect amount for group group");
        }

        /// <summary>
        /// Validates if method BankAccountTransactions returns the correct transaction total on january first of the year
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 5)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void BankAccountTransactions_05_FirstTransactionOfYear_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            int currentYear = DateTime.Now.Year;
            DateTime lastYearDate = new DateTime(currentYear - 1, 2, 2);
            DateTime firstExpectedDate = new DateTime(currentYear, 1, 1);
            DateTime startDateUtc = new DateTime(1970, 1, 1);

            decimal firstExpectedDateUtc = Convert.ToDecimal(firstExpectedDate.Subtract(startDateUtc).TotalMilliseconds);

            Transaction lastYearTransaction = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 0, date: lastYearDate, amount: 150);

            Transaction thisYearTransactionOne = TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 1, date: firstExpectedDate, amount: 150);
            
            // Act
            ActionResult result = graphController.BalanceHistoryPerYear(currentYear, new[] { TestData.DefaultCheckingBankAccount.Id });

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsTrue(jsonData.Success, "Success should be set to true");
            Assert.IsNotNull(jsonData.ObjectData, "Object data should be set");
            Assert.IsInstanceOf< List<decimal[]>>(jsonData.ObjectData, "Incorrect data type returned");

            IList<decimal[]> transactions = (List<decimal[]>)jsonData.ObjectData;
            Assert.AreEqual(1, transactions.Count, "Incorrect amount of transactions returned");

            decimal[] firstTransactionsGroup = transactions[0];
            Assert.AreEqual(firstExpectedDateUtc, firstTransactionsGroup[0], "first date is not correct");

            decimal expectedFirstGroupAmount = lastYearTransaction.Amount + thisYearTransactionOne.Amount + TestData.DefaultCheckingBankAccount.StartBalance;
            Assert.AreEqual(expected: expectedFirstGroupAmount, actual: firstTransactionsGroup[1], message: "Incorrect amount for first group");
        }

        /// <summary>
        /// Validates if method ProfitPerMonth returns the correct result when there are no transactions available
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 6)]
        public void ProfitPerMonthForYear_06_NoTransactions_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;
            int year = DateTime.Now.Year;

            // Act
            ActionResult result = graphController.ProfitPerMonthForYear(year);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsTrue(jsonData.Success, "Success should be set to true");
            Assert.IsNotNull(jsonData.ObjectData, "Object data should be set");
            Assert.IsInstanceOf< List<decimal>>(jsonData.ObjectData, "Incorrect data type returned");

            List<decimal> profitList = (List<decimal>) jsonData.ObjectData;
            Assert.AreEqual(12, profitList.Count, "Profit list should always contain 12 months of data");
            Assert.IsFalse(profitList.Any(item => item != 0), "All entries should contain zero for profit");
        }

        /// <summary>
        /// Validates if method ProfitPerMonth returns the correct result when the year has gaps in the transaction months
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 7)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ProfitPerMonthForYear_07_PartialTransactions_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;
            int year = DateTime.Now.Year;

            Transaction februaryTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id, TestData.DefaultTestApplicationUser.Id, index: 0, amount: 150,
                date: new DateTime(year, 2, 14));

            Transaction decemberTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id, TestData.DefaultTestApplicationUser.Id, index: 1, amount: 200,
                date: new DateTime(year, 12, 12));

            // Act
            ActionResult result = graphController.ProfitPerMonthForYear(year);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsTrue(jsonData.Success, "Success should be set to true");
            Assert.IsNotNull(jsonData.ObjectData, "Object data should be set");
            Assert.IsInstanceOf< List<decimal>>(jsonData.ObjectData, "Incorrect data type returned");

            List<decimal> profitList = (List<decimal>) jsonData.ObjectData;
            Assert.AreEqual(12, profitList.Count, "Profit list should always contain 12 months of data");

            Assert.AreEqual(1, profitList.IndexOf(februaryTransaction.Amount), "february was placed at wrong position");
            Assert.AreEqual(11, profitList.IndexOf(decemberTransaction.Amount), "december was placed a wrong position");
            Assert.AreEqual(10, profitList.Count(item => item == 0), "All other entries should be zero" );
        }

        /// <summary>
        /// Validates if method ProfitPerMonth returns the correct result when there are transactions for all months available
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 8)]
        public void ProfitPerMonthForYear_08_AllMonthsTransactions_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;
            int year = DateTime.Now.Year;

            // Upsert a transaction for the year before to check if it wont pop up in the profit amount
            TestData.UpsertTransaction(bankAccountId: TestData.DefaultCheckingBankAccount.Id,
                applicationUserId: TestData.DefaultTestApplicationUser.Id, index: 0, amount: 10000, date: new DateTime(year - 1, 5, 5));

            List<Transaction> upsertedPositiveTransactions = new List<Transaction>();
            for (int month = 1; month <= 12; month++)
            {
                upsertedPositiveTransactions.Add(TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id, TestData.DefaultTestApplicationUser.Id, index: month,
                    amount: month + 200, date: new DateTime(year, month, 1)));
            }

            List<Transaction> upsertedNegativeTransactions = new List<Transaction>();
            for (int month = 1; month <= 12; month++)
            {
                upsertedNegativeTransactions.Add(TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id, TestData.DefaultTestApplicationUser.Id, index: month + 12,
                    amount: month - 300, date: new DateTime(year, month, 20)));
            }

            ActionResult result = graphController.ProfitPerMonthForYear(year);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsTrue(jsonData.Success, "Success should be set to true");
            Assert.IsNotNull(jsonData.ObjectData, "Object data should be set");
            Assert.IsInstanceOf< List<decimal>>(jsonData.ObjectData, "Incorrect data type returned");

            List<decimal> profitList = (List<decimal>) jsonData.ObjectData;
            Assert.AreEqual(12, profitList.Count, "Profit list should always contain 12 months of data");

            
            for (int month = 1; month <= 12; month++)
            {
                Assert.AreEqual(upsertedPositiveTransactions[month - 1].Amount + upsertedNegativeTransactions[month - 1].Amount, profitList[month - 1], "Incorrect profit amount for month {0}", month);
            }
        }

        /// <summary>
        /// Validates if method BankAccountTransactions returns the correct result when there is null given for bank accounts
        /// </summary>
        [TestCase(Author = "Maarten van Arem")]
        [FinanceTestCase(FinanceTestCategory.GraphController, 9)]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void BankAccountTransactions_09_NullBankAccounts_ReturnsAll_Success()
        {
            // Arrange
            TestData.UpsertDefaultUserBankAccounts();
            BankAccount secondBankAccount = TestData.UpsertBankAccount(TestData.DefaultTestApplicationUser.Id, index: 1);
            int year = DateTime.Now.Year;

            SessionHelper.TestUserId = TestData.DefaultTestApplicationUser.Id;

            Transaction firstBankAccountTransaction = TestData.UpsertTransaction(TestData.DefaultCheckingBankAccount.Id, TestData.DefaultTestApplicationUser.Id, index: 0, amount: 200, date: new DateTime(year, 3, 3));
            Transaction secondBankAccountTransaction = TestData.UpsertTransaction(secondBankAccount.Id, TestData.DefaultTestApplicationUser.Id, index: 1, amount: 250, date: new DateTime(year, 3, 3));

            // Act
            ActionResult result = graphController.BalanceHistoryPerYear(year: year, bankAccountIds: null);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOf< JsonResult>(result, "Result is of incorrect type");

            FinanceJsonResult jsonData = (FinanceJsonResult)((JsonResult)result).Data;

            Assert.IsTrue(jsonData.Success, "Success should be set to true");
            Assert.IsNotNull(jsonData.ObjectData, "Object data should be set");
            Assert.IsInstanceOf< List<decimal[]>>(jsonData.ObjectData, "Incorrect data type returned");

            IList<decimal[]> balancePerDate = (List<decimal[]>)jsonData.ObjectData;
            Assert.AreEqual(1, balancePerDate.Count, "Incorrect amount of transactions returned");
            Assert.AreEqual(expected: firstBankAccountTransaction.Amount + secondBankAccountTransaction.Amount + TestData.DefaultCheckingBankAccount.StartBalance + secondBankAccount.StartBalance, 
                actual: balancePerDate[0][1], message: "Incorrect balance returned");
        }

        #endregion
    }
}

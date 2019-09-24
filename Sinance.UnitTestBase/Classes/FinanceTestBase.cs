using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Finances.SqlDataAccess;
using Finances.SqlDataAccess.Migrations;
using Finances.Domain.Entities;
using NUnit.Framework;

namespace Finances.UnitTestBase.Classes
{
    /// <summary>
    /// Baseclass for testing
    /// </summary>
    public class FinanceTestBase<TFinanceTestData> where TFinanceTestData : FinanceTestData, new()
    {
        /// <summary>
        /// Testdata to use for creating testdata
        /// </summary>
        public TFinanceTestData TestData { get; private set; }

        /// <summary>
        /// The current test context
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Onetime setup
        /// </summary>
        [OneTimeSetUp]
        public virtual void OnetimeSetup()
        {
            // Other initialize sequence than usual, test database needs to be created and also cleared for each test
            using (FinanceContext context = new FinanceContext())
            {
                TestDatabaseConfiguration initializeDomain = new TestDatabaseConfiguration();
                MigrateDatabaseToLatestVersion<FinanceContext, Configuration> initializeMigrations = new MigrateDatabaseToLatestVersion<FinanceContext, Configuration>();

                initializeMigrations.InitializeDatabase(context);
                initializeDomain.InitializeDatabase(context);
            }
        }

        /// <summary>
        /// Test initialization
        /// </summary>
        [SetUp]
        public virtual void Initialize()
        {
            TestData = new TFinanceTestData();

            FinanceTestCaseAttribute testCaseAttribute = DetermineTestAttributes();
            if (testCaseAttribute != null)
            {
                TestData.TestIndex = testCaseAttribute.TestIndex;
                TestData.TestCategory = testCaseAttribute.TestCategory;

                // Make sure there are no double test cases with the same index
                if (Main.CompletedTestCases.ContainsKey(testCaseAttribute.TestCategory))
                {
                    var indexList = Main.CompletedTestCases[testCaseAttribute.TestCategory];
                    if (indexList.Any(item => item == testCaseAttribute.TestIndex))
                        Assert.Fail("There is already a {0} test case with index {1}", TestData.TestCategory, TestData.TestIndex);
                    else
                        indexList.Add(TestData.TestIndex);
                }
                else
                    Main.CompletedTestCases.Add(testCaseAttribute.TestCategory, new List<int> {TestData.TestIndex});
            }
        }

        /// <summary>
        /// Determines the active running test attributes
        /// </summary>
        /// <returns>FinanceTestCaseAttribute attribute</returns>
        private FinanceTestCaseAttribute DetermineTestAttributes()
        {
            FinanceTestCaseAttribute testCaseAttribute = null;

            string testClassName = TestContext.CurrentContext.Test.ClassName;
            string testMethodName = TestContext.CurrentContext.Test.MethodName;
            
            // NOTE: You might have to use AppDomain.CurrentDomain.GetAssemblies() and then call GetTypes on each assembly if this code
            //       resides in a baseclass in another assembly. 
            Type currentClassType = GetType().Assembly.GetTypes().FirstOrDefault(type => type.FullName == testClassName);
            if (currentClassType != null)
            {
                MethodInfo currentMethod = currentClassType.GetMethod(testMethodName);

                IEnumerable<FinanceTestCaseAttribute> descriptionAttributes = currentMethod.GetCustomAttributes(typeof(FinanceTestCaseAttribute), true) as IEnumerable<FinanceTestCaseAttribute>;
                if (descriptionAttributes != null)
                {
                    testCaseAttribute = descriptionAttributes.FirstOrDefault();
                }
            }

            return testCaseAttribute;
        }

        /// <summary>
        /// Test cleanup
        /// </summary>
        [TearDown]
        public virtual void Cleanup()
        {
            TestData.Dispose();
        }

        #region Test Helper methods

        /// <summary>
        /// Compares two transactions if they are equal
        /// </summary>
        /// <param name="transactionExpected">The expected transaction</param>
        /// <param name="transactionActual">The actual transaction</param>
        /// <param name="includeIds">If the id should be checked</param>
        /// <param name="includeBankAndUser">If the bank account and user id should be checked</param>
        public void Compare_Transactions(Transaction transactionExpected, Transaction transactionActual, bool includeIds = true, bool includeBankAndUser = true)
        {
            if (transactionExpected == null)
                throw new ArgumentNullException(nameof(transactionExpected));

            if (transactionActual == null)
                throw new ArgumentNullException(nameof(transactionActual));

            if(includeIds)
                Assert.AreEqual(transactionExpected.Id, transactionActual.Id, "transaction Id is not equal");

            if (includeBankAndUser)
            {
                Assert.AreEqual(transactionExpected.ApplicationUserId, transactionActual.ApplicationUserId, "transaction ApplicationUserId is not equal");
                Assert.AreEqual(transactionExpected.BankAccountId, transactionActual.BankAccountId, "transaction BankAccountId is not equal");
            }

            Assert.AreEqual(transactionExpected.Name, transactionActual.Name, "transaction Name is not equal");
            Assert.AreEqual(transactionExpected.AccountNumber, transactionActual.AccountNumber, "transaction AccountNumber is not equal");
            Assert.AreEqual(transactionExpected.Amount, transactionActual.Amount, "transaction Amount is not equal");
            Assert.AreEqual(transactionExpected.AmountIsNegative, transactionActual.AmountIsNegative, "transaction AmountIsNegative is not equal");
            Assert.AreEqual(transactionExpected.Date, transactionActual.Date, "transaction Date is not equal");
            Assert.AreEqual(transactionExpected.Description, transactionActual.Description, "transaction Description is not equal");
            Assert.AreEqual(transactionExpected.DestinationAccount, transactionActual.DestinationAccount, "transaction DestinationAccount is not equal");
        }

        #endregion
    }
}

using Finances.Bll.Handlers;
using Finances.Domain.Entities;
using Finances.UnitTestBase.Classes;

namespace Finances.Web.Tests.Controllers
{
    /// <summary>
    /// Testdata class for setting up test data
    /// </summary>
    public class CategoryControllerTestData : FinanceTestData
    {
        internal Category Test07_Category;
        internal Transaction Test07_ExpectedTransaction;

        /// <summary>
        /// Sets up the test data for test 07
        /// </summary>
        public void Test07_TestData()
        {
            UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = DefaultTestApplicationUser.Id;

            BankAccount bankAccountTwo = UpsertBankAccount(DefaultTestApplicationUser.Id, 1);
            Test07_ExpectedTransaction = UpsertTransaction(applicationUserId: DefaultTestApplicationUser.Id, bankAccountId: DefaultCheckingBankAccount.Id, index: 0, amount: 20);
            UpsertTransaction(applicationUserId: DefaultTestApplicationUser.Id, bankAccountId: bankAccountTwo.Id, amount: 10, index: 1);

            Test07_Category = UpsertCategory(applicationUserId: DefaultTestApplicationUser.Id);
            UpsertCategoryMapping(Test07_Category, ColumnType.Name, matchValue: Test07_ExpectedTransaction.Name);
        }

        internal Category Test14_Category;
        internal Transaction Test14_ExpectedTransaction;

        /// <summary>
        /// Sets up test data for test case 14
        /// </summary>
        public void Test14_TestData()
        {
            UpsertDefaultUserBankAccounts();
            SessionHelper.TestUserId = DefaultTestApplicationUser.Id;

            Test14_ExpectedTransaction = UpsertTransaction(applicationUserId: DefaultTestApplicationUser.Id, bankAccountId: DefaultCheckingBankAccount.Id, index: 0);
            Test14_Category = UpsertCategory(DefaultTestApplicationUser.Id);

            // Upsert another transaction linked to a different category
            Transaction otherTransaction = UpsertTransaction(applicationUserId: DefaultTestApplicationUser.Id, bankAccountId: DefaultCheckingBankAccount.Id, index: 1);
            Category otherCategory = UpsertCategory(applicationUserId: DefaultTestApplicationUser.Id, index: 1);
            UpsertTransactionCategory(transactionId: otherTransaction.Id, categoryId: otherCategory.Id);

            UpsertCategoryMapping(category: Test14_Category, columnType: ColumnType.Name, matchValue: Test14_ExpectedTransaction.Name);
        }
    }
}

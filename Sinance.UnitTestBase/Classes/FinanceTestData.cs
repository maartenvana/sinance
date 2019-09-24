using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Finances.Domain.Entities;
using Finances.SqlDataAccess;
using Finances.SqlDataAccess.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NUnit.Framework;

namespace Finances.UnitTestBase.Classes
{
    /// <summary>
    /// Baseclass for test data classes
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Needs to be instantiated for generics")]
    public class FinanceTestData : IDisposable
    {
        #region Private Members

        private IGenericRepository genericRepository;
        private UserManager<ApplicationUser> userManager;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current test category
        /// </summary>
        public FinanceTestCategory TestCategory { get; internal set; }

        /// <summary>
        /// Current test index
        /// </summary>
        public int TestIndex { get; set; }

        /// <summary>
        /// Generic repository to use for database access
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in dispose")]
        public IGenericRepository GenericRepository
        {
            get
            {
                if (genericRepository == null)
                    genericRepository = new GenericRepository(new FinanceContext());

                return genericRepository;
            }
        }

        /// <summary>
        /// UserManager to use for accessing user functionality
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in dispose")]
        public UserManager<ApplicationUser> UserManager
        {
            get { return userManager ?? (userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new FinanceContext()))); }
        }

        /// <summary>
        /// Default application user to use for testing
        /// </summary>
        public ApplicationUser DefaultTestApplicationUser { get; private set; }

        /// <summary>
        /// Default bank account to use for testing
        /// </summary>
        public BankAccount DefaultCheckingBankAccount { get; private set; }

        /// <summary>
        /// Default bank account to use for testing
        /// </summary>
        public BankAccount DefaultSavingsBankAccount { get; private set; }

        /// <summary>
        /// Default transaction to use for testing
        /// </summary>
        public Transaction DefaultTransaction { get; private set; }

        /// <summary>
        /// Default category to use for testing
        /// </summary>
        public Category DefaultCategory { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Upserts a default user and bank account to the database
        /// </summary>
        public void UpsertDefaultUserBankAccounts()
        {
            DefaultTestApplicationUser = UpsertApplicationUser();
            DefaultCheckingBankAccount = UpsertBankAccount(DefaultTestApplicationUser.Id, index: 98, startBalance:100, accountType: BankAccountType.Checking);
            DefaultSavingsBankAccount = UpsertBankAccount(DefaultTestApplicationUser.Id, index: 99, startBalance:0, accountType: BankAccountType.Savings);
        }

        /// <summary>
        /// Upserts a default user, bank account, transaction and category to the database
        /// </summary>
        public void UpsertUserBankAccountTransactionCategory()
        {
            DefaultTestApplicationUser = UpsertApplicationUser();
            DefaultCheckingBankAccount = UpsertBankAccount(DefaultTestApplicationUser.Id, startBalance: 100);
            DefaultTransaction = UpsertTransaction(bankAccountId: DefaultCheckingBankAccount.Id, applicationUserId: DefaultTestApplicationUser.Id);
            DefaultCategory = UpsertCategory(applicationUserId: DefaultTestApplicationUser.Id);
        }

        /// <summary>
        /// Generates a test id for the given entity
        /// </summary>
        /// <param name="entityType">Entitytype to generate id for</param>
        /// <param name="index">Index of the id</param>
        /// <returns>Generated test id</returns>
        public int GenerateTestId(FinanceEntityType entityType, int index = 0)
        {
            if (index > 99 || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (TestCategory == 0 || TestIndex == 0)
                Assert.Fail("Test case is missing or has incorrect [FinanceTestCase] annotation");

            int testIndex = TestIndex * 100;
            int entityTypeId = (int)entityType * 10000;
            int testCategoryId = (int)TestCategory * 10000000;
            int testId = testCategoryId + entityTypeId + testIndex + index;

            Assert.IsFalse(Main.GeneratedTestIds.Any(item => item == testId), 
                "Id: {0} for entity: {1} for test case {2} of category {3} was generated twice", testId, entityType, TestIndex, TestCategory);
            Main.GeneratedTestIds.Add(testId);

            return testId;
        }

        /// <summary>
        /// Creates and upserts a new application user
        /// </summary>
        /// <param name="index">Index of the user</param>
        /// <returns>Created application user</returns>
        public ApplicationUser UpsertApplicationUser(int index = 0)
        {
            string userName = string.Format(CultureInfo.CurrentCulture, "Test_User_{0}", GenerateTestId(FinanceEntityType.ApplicationUser, index));
            ApplicationUser user = new ApplicationUser { UserName = userName };
            IdentityResult result = UserManager.Create(user);

            if (!result.Succeeded)
                Assert.Fail("Failed to create application user");

            return user;
        }

        /// <summary>
        /// Upserts a bank account to the test database
        /// </summary>
        /// <param name="userId">User account for the bank account</param>
        /// <param name="index">Index of the bank account</param>
        /// <param name="currentBalance">Current balance of the account</param>
        /// <param name="startBalance">Start balance of the bank account</param>
        /// <param name="name">Name of the transaction, leave blank for default naming</param>
        /// <param name="accountType">Type of bank account</param>
        /// <returns>Upserted bank account</returns>
        public BankAccount UpsertBankAccount(string userId, int index = 0, decimal currentBalance = 0, decimal startBalance = 0, string name = null, BankAccountType accountType = BankAccountType.Checking)
        {
            BankAccount bankAccount = CreateBankAccount(
                id: GenerateTestId(FinanceEntityType.BankAccount, index),
                currentBalance: currentBalance,
                startBalance: startBalance,
                name: name,
                userId: userId,
                accountType: accountType);

            GenericRepository.Insert(bankAccount);
            GenericRepository.Save();

            return bankAccount;
        }

        /// <summary>
        /// Upserts a transaction to the test database
        /// </summary>
        /// <param name="bankAccountId">Bank account of the transaction</param>
        /// <param name="applicationUserId">User id for the transaction</param>
        /// <param name="index">Index of the transaction</param>
        /// <param name="amount">Amount of the transaction</param>
        /// <param name="date">Date of the transaction</param>
        /// <param name="destinationAccount">Destination account of the transaction</param>
        /// <param name="name">Name of the transaction, leave blank for default naming</param>
        /// <param name="description">Description of the transaction, leave blank for default description</param>
        /// <returns>Upserted transaction</returns>
        public Transaction UpsertTransaction(int bankAccountId, string applicationUserId, int index = 0, decimal amount = 1, DateTime? date = null, string destinationAccount = null, 
            string name = null, string description = null)
        {
            Transaction transaction = CreateTransaction(id: GenerateTestId(FinanceEntityType.Transaction, index), 
                bankAccountId: bankAccountId, 
                amount: amount, 
                date: date,
                destinationAccount: destinationAccount, 
                name: name, 
                description: description,
                applicationUserId: applicationUserId);

            GenericRepository.Insert(transaction);
            GenericRepository.Save();

            return transaction;
        }

        /// <summary>
        /// Upserts a transaction category to the database
        /// </summary>
        /// <param name="index">Index for the generation of an id</param>
        /// <param name="categoryId">Category id of the link</param>
        /// <param name="transactionId">Transaction id of the link</param>
        /// <param name="amount">Amount to link</param>
        /// <returns>The upserted transaction category</returns>
        public TransactionCategory UpsertTransactionCategory(int categoryId, int transactionId, int index = 0, decimal? amount = null)
        {
            TransactionCategory transactionCategory = CreateTransactionCategory(id: GenerateTestId(FinanceEntityType.TransactionCategory, index),
                categoryId: categoryId,
                transactionId: transactionId,
                amount: amount);

            GenericRepository.Insert(transactionCategory);
            GenericRepository.Save();

            return transactionCategory;
        }

        /// <summary>
        /// Upserts a Category to the database
        /// </summary>
        /// <param name="applicationUserId">User id for the category</param>
        /// <param name="index">Index to use</param>
        /// <param name="name">Name of the category</param>
        /// <param name="parentId">Parent id of the category</param>
        /// <param name="isRegular">If the category is a regular expense</param>
        /// <param name="colorCode">Color code of the transaction</param>
        /// <returns>The upserted category</returns>
        public Category UpsertCategory(string applicationUserId, int index = 0, string name = null, int? parentId = null, bool isRegular = false, string colorCode = "#FFFFFF")
        {
            Category category = CreateCategory(id: GenerateTestId(FinanceEntityType.Category, index), 
                parentId: parentId, 
                isRegular: isRegular, 
                colorCode: colorCode, 
                name: name,
                applicationUserId: applicationUserId);

            GenericRepository.Insert(category);
            GenericRepository.Save();

            return category;
        }

        /// <summary>
        /// Upserts an automatic category mapping to the database
        /// </summary>
        /// <param name="category">Category to use</param>
        /// <param name="columnType">Column to match value</param>
        /// <param name="matchValue">Value of the column to match</param>
        /// <param name="index">Index of the mapping</param>
        /// <returns>The created category mapping</returns>
        public CategoryMapping UpsertCategoryMapping(Category category, ColumnType columnType, string matchValue = null, int index = 0)
        {
            CategoryMapping categoryMapping = CreateCategoryMapping(id: GenerateTestId(FinanceEntityType.CategoryMapping, index),
                columnTypeId: columnType,
                matchValue: matchValue,
                category: category);

            GenericRepository.Insert(categoryMapping);
            GenericRepository.Save();

            return categoryMapping;
        }

        /// <summary>
        /// Upserts an import bank
        /// </summary>
        /// <param name="index">Index to use</param>
        /// <param name="name">Name of the bank</param>
        /// <param name="delimiter">Delimiter for the import</param>
        /// <param name="containsHeader">If the import contains a header</param>
        /// <returns>The created import bank</returns>
        public ImportBank UpsertImportBank(int index = 0, string name = null, string delimiter = ",", bool containsHeader = true)
        {
            ImportBank importBank = CreateImportBank(id: GenerateTestId(FinanceEntityType.ImportBank, index), 
                name: name, 
                delimiter: delimiter, 
                containsHeader: containsHeader);

            GenericRepository.Insert(importBank);
            genericRepository.Save();

            return importBank;
        }


        /// <summary>
        /// Upserts an import mapping
        /// </summary>
        /// <param name="index">Index to use</param>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="columnType">Type of the column</param>
        /// <param name="columnName">Name of the column</param>
        /// <param name="formatValue">Format of the column</param>
        /// <param name="columnIndex">Index of the column</param>
        /// <returns>The created import mapping</returns>
        public ImportMapping UpsertImportMapping(int index, int bankAccountId, ColumnType columnType, int columnIndex, string columnName, string formatValue = null)
        {
            ImportMapping importMapping = CreateImportMapping(id: GenerateTestId(FinanceEntityType.ImportMapping, index), 
                bankAccountId: bankAccountId, 
                columnType: columnType, 
                columnName: columnName, 
                formatValue: formatValue, 
                columnIndex: columnIndex);

            GenericRepository.Insert(importMapping);
            genericRepository.Save();

            return importMapping;
        }

        /// <summary>
        /// Creates a bank account with the given parameters
        /// </summary>
        /// <param name="id">Id of the bank account</param>
        /// <param name="currentBalance">The current balance of the bank account</param>
        /// <param name="startBalance">The start balance of the bank account</param>
        /// <param name="name">Replaces the standard test name of the bank account, leave blank for default naming</param>
        /// <param name="index">Index to use for name generation</param>
        /// <param name="userId">User of the bank account</param>
        /// <param name="accountType">Type of bank account</param>
        /// <param name="disabled">If the account is disabled</param>
        /// <returns>The created bank account</returns>
        public BankAccount CreateBankAccount(int id, int index = 0, string userId = null, decimal currentBalance = 0, decimal startBalance = 0, string name = null, bool disabled = false, BankAccountType accountType = BankAccountType.Checking)
        {
            string accountName = name;
            
            if (name == null)
                accountName = string.Format(CultureInfo.CurrentCulture, "TestBankAccount_{0}", id != 0 ? id : GenerateTestId(FinanceEntityType.BankAccount, index));

            BankAccount bankAccount = new BankAccount
            {
                Id = id,
                Name = accountName,
                CurrentBalance = currentBalance,
                StartBalance = startBalance,
                ApplicationUserId = userId,
                AccountType = accountType,
                Disabled = disabled
            };

            return bankAccount;
        }

        /// <summary>
        /// Creates a category with the given parameters
        /// </summary>
        /// <param name="id">Id for the category</param>
        /// <param name="applicationUserId">User id for the category</param>
        /// <param name="name">Name of the category, leave blank for default naming</param>
        /// <param name="parentId"></param>
        /// <param name="isRegular">If the category is a regular expense</param>
        /// <param name="colorCode">Color code for the category</param>
        /// <param name="index">Index to use for name generation (if id is 0)</param>
        /// <returns>The created category</returns>
        public Category CreateCategory(int id, int index = 0, string applicationUserId = null, string name = null, int? parentId = null, bool isRegular = false, string colorCode = "#FFFFFF")
        {
            string categoryName = name;

            if (name == null)
                categoryName = string.Format(CultureInfo.CurrentCulture, "TestCategory_{0}", id != 0 ? id : GenerateTestId(FinanceEntityType.Category, index));
            
            return new Category
            {
                Id = id,
                Name = categoryName,
                IsRegular = isRegular,
                ColorCode = colorCode,
                ParentId = parentId,
                ApplicationUserId = applicationUserId
            };
        }

        /// <summary>
        /// Creates a category mapping with the given parameters
        /// </summary>
        /// <param name="id">Id of the category mapping</param>
        /// <param name="columnTypeId">For which column type the mapping is</param>
        /// <param name="category">Category to use</param>
        /// <param name="index">Index to use for name generation (if id is 0)</param>
        /// <param name="matchValue">Match value of the column</param>
        /// <returns>The created categoryMapping</returns>
        public CategoryMapping CreateCategoryMapping(int id, ColumnType columnTypeId, Category category, int index = 0, string matchValue = null)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            string categoryMatchValue = matchValue;

            if (categoryMatchValue == null)
                categoryMatchValue = string.Format(CultureInfo.CurrentCulture, "TestMatchValue_{0}", id != 0 ? id : GenerateTestId(FinanceEntityType.CategoryMapping, index));

            return new CategoryMapping
            {
                Id = id,
                CategoryId = category.Id,
                Category = category,
                ColumnTypeId = columnTypeId,
                MatchValue = categoryMatchValue
            };
        }

        /// <summary>
        /// Creates a transaction with the given parameters
        /// </summary>
        /// <param name="id">Id of the transaction</param>
        /// <param name="bankAccountId">Bank account of the transaction</param>
        /// <param name="applicationUserId">User id for the transaction</param>
        /// <param name="amount">Amount of the transaction</param>
        /// <param name="date">Date of the transaction</param>
        /// <param name="destinationAccount">Destination of the transaction</param>
        /// <param name="name">Name of the transaction, leave blank for default naming</param>
        /// <param name="description">Description of the transaction, leave blank for default description</param>
        /// <param name="accountFrom">Account where the transaction is from</param>
        /// <param name="isNegative">If the amount is negative</param>
        /// <returns>The created transaction</returns>
        public static Transaction CreateTransaction(int id, int bankAccountId, string applicationUserId = null, decimal amount = 1, DateTime? date = null, 
            string destinationAccount = null, string name = null, string description = null, string accountFrom = null, bool isNegative = false)
        {
            string transactionName = name ?? string.Format(CultureInfo.CurrentCulture, "TestTransaction_{0}", id);
            string transactionDescription = description ?? string.Format(CultureInfo.CurrentCulture, "TestTransactionDescription_{0}", id);

            if (date == null)
                date = DateTime.Now.Date;

            return new Transaction
            {
                Id = id,
                Name = transactionName,
                Amount = amount,
                Date = date.Value.Date,
                Description = transactionDescription,
                DestinationAccount = destinationAccount,
                BankAccountId = bankAccountId,
                ApplicationUserId = applicationUserId,
                AccountNumber = accountFrom,
                AmountIsNegative = isNegative
            };
        }

        /// <summary>
        /// Creates a transaction category
        /// </summary>
        /// <param name="id">Id to use</param>
        /// <param name="categoryId">Category id of the link</param>
        /// <param name="transactionId">Transaction id of the link</param>
        /// <param name="amount">Amount linked</param>
        /// <returns>The created transaction category</returns>
        public static TransactionCategory CreateTransactionCategory(int id, int categoryId, int transactionId, decimal? amount = null)
        {
            return new TransactionCategory
            {
                Id = id,
                Amount = amount,
                CategoryId = categoryId,
                TransactionId = transactionId
            };
        }

        /// <summary>
        /// Creates an import bank
        /// </summary>
        /// <param name="id">Id to use</param>
        /// <param name="name">Name of the bank</param>
        /// <param name="delimiter">Delimiter for the import</param>
        /// <param name="containsHeader">If the import contains a header</param>
        /// <returns>The created import bank</returns>
        public static ImportBank CreateImportBank(int id, string name, string delimiter, bool containsHeader)
        {
            if (name == null)
                name = string.Format(CultureInfo.CurrentCulture, "TestImportBank_{0}", id);

            return new ImportBank
            {
                Id = id,
                Name = name,
                Delimiter = delimiter,
                ImportContainsHeader = containsHeader
            };
        }

        /// <summary>
        /// Creates an import mapping
        /// </summary>
        /// <param name="id">Id to use</param>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="columnType">Type of the column</param>
        /// <param name="columnName">Name of the column</param>
        /// <param name="formatValue">Format of the column</param>
        /// <param name="columnIndex">Index of the column</param>
        /// <returns>The created import mapping</returns>
        public static ImportMapping CreateImportMapping(int id, int bankAccountId, ColumnType columnType, int columnIndex, 
            string columnName, string formatValue = null)
        {
            return new ImportMapping
            {
                Id = id,
                ImportBankId = bankAccountId,
                ColumnTypeId = columnType,
                ColumnName = columnName,
                FormatValue = formatValue,
                ColumnIndex = columnIndex
            };
        }

        #endregion

        /// <summary>
        /// Disposes the current class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current class
        /// </summary>
        /// <param name="disposing">If we are disposing or not</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (genericRepository != null)
                    genericRepository.Dispose();

                if (userManager != null)
                    userManager.Dispose();
            }
        }
    }
}
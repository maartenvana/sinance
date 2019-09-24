using System;
using System.Data.Entity;
using Finances.SqlDataAccess;

namespace Finances.UnitTestBase.Classes
{
    /// <summary>
    /// Class containing the configuration for automatic migrations
    /// </summary>
    public sealed class TestDatabaseConfiguration : CreateDatabaseIfNotExists<FinanceContext>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TestDatabaseConfiguration() : base()
        {
            
        }

        /// <summary>
        /// Initializes the database
        /// </summary>
        /// <param name="context">Context to use</param>
        public override void InitializeDatabase(FinanceContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.InitializeDatabase(context);

            context.Database.ExecuteSqlCommand("DELETE FROM [ImportMapping]");
            context.Database.ExecuteSqlCommand("DELETE FROM [ImportBank]");
            context.Database.ExecuteSqlCommand("DELETE FROM [TransactionCategory]");
            context.Database.ExecuteSqlCommand("DELETE FROM [Transaction]");
            context.Database.ExecuteSqlCommand("DELETE FROM [CategoryMapping]");
            context.Database.ExecuteSqlCommand("DELETE FROM [Category] WHERE ParentId IS NOT NULL");
            context.Database.ExecuteSqlCommand("DELETE FROM [Category] WHERE ParentId IS NULL");
            context.Database.ExecuteSqlCommand("DELETE FROM [AspNetUserClaims]");
            context.Database.ExecuteSqlCommand("DELETE FROM [AspNetUserLogins]");
            context.Database.ExecuteSqlCommand("DELETE FROM [AspNetUserRoles]");
            context.Database.ExecuteSqlCommand("DELETE FROM [AspNetRoles]");
            context.Database.ExecuteSqlCommand("DELETE FROM [AspNetUsers]");
        }

        /// <summary>
        /// Seeds the testdatabase with data
        /// </summary>
        /// <param name="context">Database context to use</param>
        protected override void Seed(FinanceContext context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            base.Seed(context);
        }
    }
}
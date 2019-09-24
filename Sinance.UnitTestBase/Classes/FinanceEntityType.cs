using System.Diagnostics.CodeAnalysis;

namespace Finances.UnitTestBase.Classes
{
    /// <summary>
    /// Enumeration for finance entity types
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum FinanceEntityType
    {
        /// <summary>
        /// None type
        /// </summary>
        None = 0,

        /// <summary>
        /// Bank account type
        /// </summary>
        BankAccount = 1,

        /// <summary>
        /// Category type
        /// </summary>
        Category = 2,

        /// <summary>
        /// Transaction type
        /// </summary>
        Transaction = 3,

        /// <summary>
        /// Category mapping type
        /// </summary>
        CategoryMapping = 4,
        
        /// <summary>
        /// Transaction category type
        /// </summary>
        TransactionCategory = 6,

        /// <summary>
        /// Application user type
        /// </summary>
        ApplicationUser = 7,

        /// <summary>
        /// Import bank type
        /// </summary>
        ImportBank = 8,

        /// <summary>
        /// Import mapping type
        /// </summary>
        ImportMapping = 9
    }
}

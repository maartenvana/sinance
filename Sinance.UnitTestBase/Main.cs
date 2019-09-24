using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Finances.UnitTestBase.Classes;

namespace Finances.UnitTestBase
{
    /// <summary>
    /// Main test method
    /// </summary>
    public static class Main
    {
        #region Private members

        private static IDictionary<FinanceTestCategory, IList<int>> completedTestCases;
        private static IList<int> generatedTestIds;

        #endregion

        #region Public Properties

        /// <summary>
        /// Dictionary containing all the runned tests
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is fine")]
        public static IDictionary<FinanceTestCategory, IList<int>> CompletedTestCases
        {
            get { return completedTestCases ?? (completedTestCases = new Dictionary<FinanceTestCategory, IList<int>>()); }
        }
        
        /// <summary>
        /// List containing all the generated ids
        /// </summary>
        public static IList<int> GeneratedTestIds
        {
            get { return generatedTestIds ?? (generatedTestIds = new List<int>()); }
        }

        #endregion
    }
}

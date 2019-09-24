using System;

namespace Finances.UnitTestBase.Classes
{
    /// <summary>
    /// Finance test case
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class FinanceTestCaseAttribute : Attribute
    {
        #region Private members

        private readonly int testIndex;
        private readonly FinanceTestCategory testCategory;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current test category
        /// </summary>
        public FinanceTestCategory TestCategory
        {
            get
            {
                return testCategory;
            }
        }

        /// <summary>
        /// The current test index
        /// </summary>
        public int TestIndex
        {
            get
            {
                return testIndex;
            }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="testCategory">Test category</param>
        /// <param name="testIndex">Test index</param>
        public FinanceTestCaseAttribute(FinanceTestCategory testCategory, int testIndex)
        {
            this.testCategory = testCategory;
            this.testIndex = testIndex;
        }
    }
}

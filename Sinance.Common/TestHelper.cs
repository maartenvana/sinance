using System;
using System.Linq;

namespace Sinance.Common
{
    /// <summary>
    /// Test helper for unit testing
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Determines if the current assembly is a test assembly
        /// </summary>
        /// <returns>If the current assembly is a test assembly</returns>
        public static bool IsTestAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(item => item.FullName.IndexOf("nunit.framework", StringComparison.OrdinalIgnoreCase) > -1);
        }
    }
}
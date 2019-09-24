using System.Web.Mvc;
using Finances.Bll.Handlers;
using NUnit.Framework;

namespace Finances.Web.Tests.Controllers
{
    /// <summary>
    /// Helper methods for controller tests
    /// </summary>
    public static class ControllerTestHelper
    {
        /// <summary>
        /// Asserts the temporary message inside of an tempdata dictionary
        /// </summary>
        /// <param name="tempData">Temporary data dictionary to use</param>
        /// <param name="expectedMessageState">Expected message state</param>
        /// <param name="expectedMessage">Expected message</param>
        public static void AssertTemporaryMessage(TempDataDictionary tempData, MessageState expectedMessageState, string expectedMessage)
        {
            Assert.AreEqual(expectedMessage, SessionHelper.RetrieveTemporaryMessage(tempData), "Incorrect temporary message");
            Assert.AreEqual(expectedMessageState, SessionHelper.RetrieveTemporaryMessageState(tempData), "Incorrect temporary message");
        }
    }
}

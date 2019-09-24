using Sinance.Web.Model;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace Sinance.Web.Helper
{
    /// <summary>
    /// Session helper containing all variables and actions related to the session of the current user
    /// </summary>
    public class TempDataHelper
    {
        /// <summary>
        /// Retrieves the message from the tempdata
        /// </summary>
        /// <param name="tempData">Tempdata to use</param>
        /// <returns>The message</returns>
        public static string RetrieveTemporaryMessage(ITempDataDictionary tempData)
        {
            if (tempData == null)
                throw new ArgumentNullException(nameof(tempData));

            string message = null;
            if (tempData.ContainsKey("Message"))
                message = (string)tempData["Message"];

            return message;
        }

        /// <summary>
        /// Retrieves the message state from the tempdata
        /// </summary>
        /// <param name="tempData">Tempdata to use</param>
        /// <returns>Message state</returns>
        public static MessageState RetrieveTemporaryMessageState(ITempDataDictionary tempData)
        {
            if (tempData == null)
                throw new ArgumentNullException(nameof(tempData));

            MessageState messageState = MessageState.None;
            if (tempData.ContainsKey("MessageState"))
                messageState = (MessageState)tempData["MessageState"];

            return messageState;
        }

        /// <summary>
        /// Sets the temporary data message and state
        /// </summary>
        /// <param name="tempData">Temporary data dictionary to use</param>
        /// <param name="state">State to set</param>
        /// <param name="message">Message to set</param>
        public static void SetTemporaryMessage(ITempDataDictionary tempData, MessageState state, string message)
        {
            if (tempData == null)
                throw new ArgumentNullException(nameof(tempData));

            if (tempData.ContainsKey("MessageState"))
                tempData["MessageState"] = state;
            else
                tempData.Add("MessageState", state);

            if (tempData.ContainsKey("Message"))
                tempData["MessageState"] = message;
            else
                tempData.Add("Message", message);
        }
    }
}
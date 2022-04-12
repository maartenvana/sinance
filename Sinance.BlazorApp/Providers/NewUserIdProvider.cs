using Sinance.Infrastructure;

namespace Sinance.BlazorApp.Providers
{
    public class NewUserIdProvider : IUserIdProvider
    {
        public NewUserIdProvider()
        {
        }

        /// <summary>
        /// Returns the current logged in user id
        /// </summary>
        public int GetCurrentUserId()
        {
            // TODO Actually do this by logging in and stuff
            return 4;
        }
    }
}

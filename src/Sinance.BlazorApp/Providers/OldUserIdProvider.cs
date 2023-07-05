using Sinance.Storage;

namespace Sinance.BlazorApp.Providers;

public class OldUserIdProvider : IUserIdProvider
{
    public OldUserIdProvider()
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
